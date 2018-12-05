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
        private byte[] signedContent;

        [SetUp]
        public void SetUp()
        {
            // Постарайтесь вынести в SetUp всё неспецифическое конфигурирование так,
            // чтобы в конкретных тестах осталась только специфика теста,
            // без конфигурирования "обычного" сценария работы

            file = new File("someFile", new byte[] {1, 2, 3});
            signedContent = new byte[] {1, 7};

            cryptographer = A.Fake<ICryptographer>();
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new FileSender(cryptographer, sender, recognizer);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
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
        [Ignore("Not implemented")]
        public void Skip_WhenBadFormat()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void Skip_WhenOlderThanAMonth()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void Send_WhenYoungerThanAMonth()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void Skip_WhenSendFails()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void Skip_WhenNotRecognized()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void IndependentlySend_WhenSeveralFilesAndSomeAreInvalid()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Ignore("Not implemented")]
        public void IndependentlySend_WhenSeveralFilesAndSomeCouldNotSend()
        {
            throw new NotImplementedException();
        }
    }
}
