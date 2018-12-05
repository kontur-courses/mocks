using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using FileSender.Dependencies;
using FluentAssertions;
using NUnit.Framework;

namespace FileSender.Solved
{
    [TestFixture]
    public class FileSender_Test
    {
        private const string Format40 = "4.0";
        private const string Format31 = "3.1";

        private FileSender fileSender;
        private ICryptographer cryptographer;
        private ISender sender;
        private IRecognizer recognizer;

        private DateTime now;
        private string fileName;
        private X509Certificate certificate;

        [SetUp]
        public void SetUp()
        {
            fileName = "some.txt";
            now = DateTime.Now;
            certificate = new X509Certificate();

            recognizer = A.Fake<IRecognizer>();

            cryptographer = A.Fake<ICryptographer>();
            A.CallTo(() => cryptographer.Sign(A<byte[]>._, certificate))
                .ReturnsLazily(GetNewBytes);

            sender = A.Fake<ISender>();
            A.CallTo(() => sender.TrySend(A<byte[]>._))
                .Returns(true);

            fileSender = new FileSender(cryptographer, sender, recognizer);
        }

        [TestCase(Format40)]
        [TestCase(Format31)]
        public void Send_WhenGoodFormat(string goodFormat)
        {
            var document = new Document(fileName, GetNewBytes(), now, goodFormat);
            var file = GetFileRecognizedTo(document);
            var signedContent = GetSigned(document);

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEmpty();
            A.CallTo(() => sender.TrySend(signedContent)).MustHaveHappened();
        }

        [Test]
        public void Skip_WhenBadFormat()
        {
            const string badFormat = "2.0";
            var document = new Document(fileName, GetNewBytes(), now, badFormat);
            var file = GetFileRecognizedTo(document);

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEquivalentTo(file);
            A.CallTo(() => sender.TrySend(A<byte[]>._)).MustNotHaveHappened();
        }

        [Test]
        public void Skip_WhenOlderThanAMonth()
        {
            var moreThanMonthAgo = now.AddMonths(-1).AddMinutes(-1);
            var document = new Document(fileName, GetNewBytes(), moreThanMonthAgo, Format40);
            var file = GetFileRecognizedTo(document);

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEquivalentTo(file);
            A.CallTo(() => sender.TrySend(A<byte[]>._)).MustNotHaveHappened();
        }

        [Test]
        public void Send_WhenYoungerThanAMonth()
        {
            var lessThanMonthAgo = now.AddMonths(-1).AddMinutes(1);
            var document = new Document(fileName, GetNewBytes(), lessThanMonthAgo, Format40);
            var file = GetFileRecognizedTo(document);
            var signedContent = GetSigned(document);

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEmpty();
            A.CallTo(() => sender.TrySend(signedContent)).MustHaveHappened();
        }

        [Test]
        public void Skip_WhenSendFails()
        {
            var document = new Document(fileName, GetNewBytes(), now, Format40);
            var file = GetFileRecognizedTo(document);
            A.CallTo(() => sender.TrySend(A<byte[]>._))
                .Returns(false);

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void Skip_WhenNotRecognized()
        {
            var file = new File(fileName, GetNewBytes());

            var actual = fileSender.SendFiles(new[] { file }, certificate);

            actual.SkippedFiles.Should().BeEquivalentTo(file);
            A.CallTo(() => sender.TrySend(A<byte[]>._)).MustNotHaveHappened();
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeAreInvalid()
        {
            var invalidDocument = new Document(fileName, GetNewBytes(), now, "2.0");
            var invalidDocumentSignedContent = GetSigned(invalidDocument);
            var documents = new[]
            {
                new Document(fileName, GetNewBytes(), now, Format40),
                invalidDocument,
                new Document(fileName, GetNewBytes(), now, Format40),
            };
            var files = documents.Select(GetFileRecognizedTo).ToArray();

            var actual = fileSender.SendFiles(files, certificate);

            A.CallTo(() => sender.TrySend(A<byte[]>._)).MustHaveHappened(Repeated.Exactly.Twice);
            A.CallTo(() => sender.TrySend(invalidDocumentSignedContent)).MustNotHaveHappened();
            actual.SkippedFiles.Should().BeEquivalentTo(files[1]);
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeCouldNotSend()
        {
            var documents = new[]
            {
                new Document(fileName, GetNewBytes(), now, Format40),
                new Document(fileName, GetNewBytes(), now, Format40),
                new Document(fileName, GetNewBytes(), now, Format40),
            };
            var files = documents.Select(GetFileRecognizedTo).ToArray();
            A.CallTo(() => sender.TrySend(A<byte[]>._))
                .ReturnsNextFromSequence(false, true, false);

            var actual = fileSender.SendFiles(files, certificate);

            actual.SkippedFiles.Should().BeEquivalentTo(files[0], files[2]);
        }

        private File GetFileRecognizedTo(Document document)
        {
            var result = new File(document.Name, document.Content);

            A.CallTo(() => recognizer.TryRecognize(result, out document))
                .Returns(true);

            return result;
        }

        private byte[] GetSigned(Document document)
        {
            var signedContent = GetNewBytes();

            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);

            return signedContent;
        }

        private static byte[] GetNewBytes() => Guid.NewGuid().ToByteArray();
    }
}