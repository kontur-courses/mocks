using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using FileSender.Dependencies;
using FluentAssertions;
using NUnit.Framework;

namespace FileSender
{
    public class FileSender
    {
        private readonly ICryptographer cryptographer;
        private readonly ISender sender;
        private readonly IRecognizer recognizer;

        public FileSender(
            ICryptographer cryptographer,
            ISender sender,
            IRecognizer recognizer)
        {
            this.cryptographer = cryptographer;
            this.sender = sender;
            this.recognizer = recognizer;
        }

        public Result SendFiles(File[] files, X509Certificate certificate)
        {
            return new Result
            {
                SkippedFiles = files
                    .Where(file => !TrySendFile(file, certificate))
                    .ToArray()
            };
        }

        private bool TrySendFile(File file, X509Certificate certificate)
        {
            Document document;
            if (!recognizer.TryRecognize(file, out document))
                return false;
            if (!CheckFormat(document) || !CheckActual(document))
                return false;
            var signedContent = cryptographer.Sign(document.Content, certificate);
            return sender.TrySend(signedContent);
        }

        private bool CheckFormat(Document document)
        {
            return document.Format == "4.0" ||
                   document.Format == "3.1";
        }

        private bool CheckActual(Document document)
        {
            return document.Created.AddMonths(1) > DateTime.Now;
        }

        public class Result
        {
            public File[] SkippedFiles { get; set; }
        }
    }

    //TODO: реализовать недостающие тесты
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
        private byte[] signedContent;
        private byte[] signedContent2;
        //private Document defaultDocument;

        [SetUp]
        public void SetUp()
        {
            // Постарайтесь вынести в SetUp всё неспецифическое конфигурирование так,
            // чтобы в конкретных тестах осталась только специфика теста,
            // без конфигурирования "обычного" сценария работы

            file = new File("someFile", new byte[] { 1, 2, 3 });
            file2 = new File("someFile2", new byte[] { 1, 2, 3, 4 });
            //var document = new Document(file.Name, file.Content, DateTime.Now, "4.0");
            signedContent = new byte[] { 1, 7 };
            signedContent2 = new byte[] { 1, 7, 2 };

            cryptographer = A.Fake<ICryptographer>();
            A.CallTo(() => cryptographer.Sign(file.Content, certificate))
                .Returns(signedContent);
            A.CallTo(() => cryptographer.Sign(file2.Content, certificate))
                .Returns(signedContent2);

            sender = A.Fake<ISender>();
            A.CallTo(() => sender.TrySend(signedContent))
                .WithAnyArguments()
                .Returns(true);

            var document = new Document(file.Name, file.Content, DateTime.Now, "4.0");
            var document2 = new Document(file2.Name, file2.Content, DateTime.Now, "4.0");
            recognizer = A.Fake<IRecognizer>();
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => recognizer.TryRecognize(file2, out document2))
                .Returns(true);

            fileSender = new FileSender(cryptographer, sender, recognizer);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

        [Test]
        public void Skip_WhenBadFormat()
        {
            string format = "3.0";
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().NotBeEmpty();
        }

        [Test]
        public void Skip_WhenOlderThanAMonth()
        {
            var document = new Document(file.Name, file.Content, DateTime.Now.AddMonths(-1).AddSeconds(-1), "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().NotBeEmpty();
        }

        [Test]
        public void Send_WhenYoungerThanAMonth()
        {
            var document = new Document(file.Name, file.Content, DateTime.Now.AddMonths(-1).AddSeconds(1), "4.0");
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

        [Test]
        public void Skip_WhenSendFails()
        {
            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(false);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().NotBeEmpty();
        }

        [Test]
        public void Skip_WhenNotRecognized()
        {
            Document _ = null;
            A.CallTo(() => recognizer.TryRecognize(file, out _))
                .Returns(false);

            fileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().NotBeEmpty();
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeAreInvalid()
        {
            Document _ = null;
            A.CallTo(() => recognizer.TryRecognize(file2, out _))
                .Returns(false);

            fileSender.SendFiles(new[] { file, file2 }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file2);
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeCouldNotSend()
        {
            A.CallTo(() => sender.TrySend(signedContent2))
                .Returns(false);

            fileSender.SendFiles(new[] { file, file2 }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file2);
        }
    }
}
