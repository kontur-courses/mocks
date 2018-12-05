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

        [SetUp]
        public void SetUp()
        {
            // Тут мы задаем некоторые известны для всех тестов данные 
            // и умолчательные поведения сервисов-заглушек.
            // Наша цель — сделать так, чтобы в конкретных тестах осталась только их специфика,
            // а конфигурирование "обычного" поведения не надо было повторять от теста к тесту
            cryptographer = A.Fake<ICryptographer>();
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new FileSender(cryptographer, sender, recognizer);

            var signedContent = Guid.NewGuid().ToByteArray();
            A.CallTo(() => cryptographer.Sign(null, null))
                .WithAnyArguments()
                .Returns(signedContent);
            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(true);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            File someFile = CreateDocumentFile(DateTime.Now, format);

            AssertSentSuccessful(someFile);
        }

        [Test]
        public void Send_WhenYoungerThanAMonth()
        {
            var almostMonthAgo = DateTime.Now.AddMonths(-1).AddDays(1);
            var someFile = CreateDocumentFile(almostMonthAgo);
            AssertSentSuccessful(someFile);
        }

        [Test]
        public void Skip_WhenBadFormat()
        {
            var someFile = CreateDocumentFile(DateTime.Now, "2.0");
            AssertCanNotBeSent(someFile);
        }

        [Test]
        public void Skip_WhenOlderThanAMonth()
        {
            var someFile = CreateDocumentFile(
                DateTime.Now.Date.AddMonths(-1).AddDays(-1));

            AssertCanNotBeSent(someFile);
        }

        [Test]
        public void Skip_WhenSendFails()
        {
            var someFile = CreateSomeGoodDocumentFile();
            A.CallTo(() => sender.TrySend(null))
                .WithAnyArguments().Returns(false);

            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
        }

        [Test]
        public void Skip_WhenNotRecognized()
        {
            var file = CreateSomeGoodDocumentFile();
            Document document;
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(false);
            AssertCanNotBeSent(file);
        }

        [Test]
        public void IndependentlySendSeveralFiles_WhenSomeFailedToSend()
        {
            var file1 = CreateSomeGoodDocumentFile();
            var file2 = CreateSomeGoodDocumentFile();
            var file3 = CreateSomeGoodDocumentFile();

            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .ReturnsNextFromSequence(false, true, false);

            var res = fileSender.SendFiles(new[] { file1, file2, file3 }, certificate);

            res.SkippedFiles
                .Should().Equal(file1, file3);
        }

        [Test]
        public void IndependentlySendSeveralFiles_WhenSomeCantBeRecognized()
        {
            var file1 = CreateSomeGoodDocumentFile();
            var file2 = CreateSomeGoodDocumentFile();
            var file3 = CreateSomeGoodDocumentFile();

            Document document;
            A.CallTo(() => recognizer.TryRecognize(file2, out document))
                .Returns(false);

            var res = fileSender.SendFiles(new[] { file1, file2, file3 }, certificate);

            res.SkippedFiles
                .Should().Equal(file2);
            A.CallTo(() => sender.TrySend(null)).WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        private File CreateSomeGoodDocumentFile()
        {
            return CreateDocumentFile(DateTime.Now);
        }

        private File CreateDocumentFile(DateTime created, string format = "4.0")
        {
            var file = new File(
                Guid.NewGuid().ToString("N"), 
                Guid.NewGuid().ToByteArray());
            var document = new Document(file.Name, file.Content, created, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true)
                .AssignsOutAndRefParameters(document);
            return file;
        }

        private void AssertSentSuccessful(File someFile)
        {
            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEmpty();
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustHaveHappened();
        }

        private void AssertCanNotBeSent(File someFile)
        {
            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();
        }

    }
}