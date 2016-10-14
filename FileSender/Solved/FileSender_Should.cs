using System;
using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using FileSender.Dependencies;
using FluentAssertions;
using NUnit.Framework;

namespace FileSender.Solved
{
    [TestFixture]
    public class FileSender_Should
    {
        private FileSender fileSender;
        private ICryptographer cryptographer;
        private ISender sender;
        private IRecognizer recognizer;

        private readonly X509Certificate certificate = new X509Certificate();
        private File file;
        private File file2;
        private File file3;
        private byte[] signedContent;
        private byte[] signedContent3;

        [SetUp]
        public void SetUp()
        {
            file = new File("someFile", new byte[] { 1, 2, 3 });
            file2 = new File("someFile2", new byte[] { 2, 3, 4 });
            file3 = new File("someFile3", new byte[] { 3, 4, 5 });
            signedContent = new byte[] { 1, 7 };
            signedContent3 = new byte[] { 3, 7 };

            cryptographer = A.Fake<ICryptographer>();
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new FileSender(cryptographer, sender, recognizer);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            var document = BuildDocument(file, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);
            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(true);

            fileSender.SendFiles(new[] {file}, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

        [Test]
        public void Skip_WhenBadFormat()
        {
            var document = BuildDocument(file, DateTime.Now, "2.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void Skip_WhenOlderThanAMonth()
        {
            var document = BuildDocument(file,
                DateTime.Now.Date.AddMonths(-1).AddDays(-1), "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void Send_WhenYoungerThanAMonth()
        {
            var document = BuildDocument(file,
                DateTime.Now.AddMonths(-1).AddDays(1), "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);

            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(true);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

        [Test]
        public void Skip_WhenSendFails()
        {
            var document = BuildDocument(file, DateTime.Now, "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(false);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void Skip_WhenNotRecognized()
        {
            Document document;
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(false);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void IndependentlySend_WhenSeveralFiles()
        {
            //NOTE: Важно задавать, сколько раз должен возвращаться каждый из результатов
            var document1 = BuildDocument(file, DateTime.Now, "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document1))
                .Returns(true).Once();

            var document2 = BuildDocument(file2, DateTime.Now, "2.0");
            A.CallTo(() => recognizer.TryRecognize(file2, out document2))
                .Returns(true).Once();

            var document3 = BuildDocument(file3, DateTime.Now, "4.0");
            A.CallTo(() => recognizer.TryRecognize(file3, out document3))
                .Returns(true).Once();

            A.CallTo(() => cryptographer.Sign(document1.Content, certificate))
                .Returns(signedContent).Once();
            A.CallTo(() => cryptographer.Sign(document3.Content, certificate))
                .Returns(signedContent3).Once();

            //NOTE: Последовательность результатов
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .ReturnsNextFromSequence(false, true);

            fileSender.SendFiles(new[] { file, file2, file3 }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file, file2);
        }

        private Document BuildDocument(File file, DateTime created, string format)
        {
            return new Document(file.Name, file.Content, created, format);
        }
    }
}