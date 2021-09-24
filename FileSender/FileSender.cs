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
            var successSend = sender.TrySend(signedContent);

            return successSend;
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
            A.CallTo(() => cryptographer.Sign(file.Content, certificate)).Returns(signedContent);
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new FileSender(cryptographer, sender, recognizer);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            // Arrange
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(true);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(true);

            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().BeEmpty();
            A.CallTo(() => sender.TrySend(signedContent)).MustHaveHappenedOnceExactly();
        }

        [TestCase("0.1")]
        [TestCase("2.3")]
        [TestCase("10.10.10")]
        public void Skip_WhenBadFormat(string format)
        {
            // Arrange
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(true);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(true);

            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().HaveCount(1);
            A.CallTo(() => sender.TrySend(signedContent)).MustNotHaveHappened();
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Skip_WhenOlderThanAMonth(string format)
        {
            var olderThanMonthDate = DateTime.Now.AddMonths(-1).AddSeconds(-30);
            
            var document = new Document(file.Name, file.Content, olderThanMonthDate, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(true);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(true);

            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().HaveCount(1);
            A.CallTo(() => sender.TrySend(signedContent)).MustNotHaveHappened();
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenYoungerThanAMonth(string format)
        {
            // Arrange
            var youngerThanAMonthDate = DateTime.Now.AddMonths(-1).AddSeconds(30);
            
            var document = new Document(file.Name, file.Content, youngerThanAMonthDate, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(true);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(true);

            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().BeEmpty();
            A.CallTo(() => sender.TrySend(signedContent)).MustHaveHappenedOnceExactly();
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Skip_WhenSendFails(string format)
        {
            // Arrange
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(true);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(false);

            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().HaveCount(1);
            A.CallTo(() => sender.TrySend(signedContent)).MustHaveHappenedOnceExactly();
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Skip_WhenNotRecognized(string format)
        {
            // Arrange
            var document = new Document(file.Name, file.Content, DateTime.Now, format);
            A.CallTo(() => recognizer.TryRecognize(file, out document)).Returns(false);
            A.CallTo(() => sender.TrySend(signedContent)).Returns(true);
            
            // Act
            var result = fileSender.SendFiles(new[] { file }, certificate);
            
            // Assert
            result.SkippedFiles.Should().HaveCount(1);
            A.CallTo(() => sender.TrySend(signedContent)).MustNotHaveHappened();
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeAreInvalid()
        {
            // Arrange
            File[] files =
            {
                new File("valid", new byte[] { 1 }),
                new File("outdated", new byte[] { 2 }),
                new File("invalidVersion", new byte[] { 3 })
            };
            
            var validDoc = new Document(file.Name, file.Content, DateTime.Now, "4.0");
            var outDatedDoc = new Document(file.Name, file.Content, DateTime.Now.AddMonths(-2), "4.0");
            var invalidVersionDoc = new Document(file.Name, file.Content, DateTime.Now, "0.0.0.0");
            
            Document _;
            A.CallTo(() => recognizer.TryRecognize(files[0], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(validDoc);
            
            A.CallTo(() => recognizer.TryRecognize(files[1], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(outDatedDoc);

            A.CallTo(() => recognizer.TryRecognize(files[2], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(invalidVersionDoc);

            A.CallTo(() => cryptographer.Sign(A<byte[]>.Ignored, certificate)).Returns(signedContent);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored)).Returns(true);


            // Act
            var result = fileSender.SendFiles(files, certificate);
            
            // Assert
            result.SkippedFiles.Should().BeEquivalentTo(files[1], files[2]);
            A.CallTo(() => cryptographer.Sign(A<byte[]>.Ignored, certificate)).MustHaveHappenedOnceExactly();
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void IndependentlySend_WhenSeveralFilesAndSomeCouldNotSend()
        {
            // Arrange
            File[] files =
            {
                new File("file1", new byte[] { 1 }),
                new File("file2", new byte[] { 2 }),
                new File("file3", new byte[] { 3 })
            };
            
            var validDoc1 = new Document(file.Name, files[0].Content, DateTime.Now, "4.0");
            var validDoc2 = new Document(file.Name, files[1].Content, DateTime.Now, "4.0");
            var validDoc3 = new Document(file.Name, files[2].Content, DateTime.Now, "4.0");
            
            Document _;
            A.CallTo(() => recognizer.TryRecognize(files[0], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(validDoc1);
            
            A.CallTo(() => recognizer.TryRecognize(files[1], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(validDoc2);

            A.CallTo(() => recognizer.TryRecognize(files[2], out _))
                .Returns(true)
                .AssignsOutAndRefParameters(validDoc3);

            A.CallTo(() => cryptographer.Sign(A<byte[]>.Ignored, certificate)).ReturnsLazily(c => (byte[])c.Arguments[0]);
            
            A.CallTo(() => sender.TrySend(files[0].Content)).Returns(true);
            A.CallTo(() => sender.TrySend(files[1].Content)).Returns(false);
            A.CallTo(() => sender.TrySend(files[2].Content)).Returns(false);

            // Act
            var result = fileSender.SendFiles(files, certificate);
            
            // Assert
            result.SkippedFiles.Should().BeEquivalentTo(files[1], files[2]);
            A.CallTo(() => cryptographer.Sign(A<byte[]>.Ignored, certificate)).MustHaveHappened(3, Times.Exactly);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored)).MustHaveHappened(3, Times.Exactly);
        }
    }
}
