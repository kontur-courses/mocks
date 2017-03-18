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
        private File someFile;
        private File someFile2;
        private File someFile3;
        private byte[] signedContent;
        private byte[] signedContent3;

        [SetUp]
        public void SetUp()
        {
			// Тут мы задаем некоторые известны для всех тестов данные 
			// и умолчательные поведения сервисов-заглушек.
			// Наша цель — сделать так, чтобы в конкретных тестах осталась только их специфика,
			// а конфигурирование "обычного" поведения не надо было повторять от теста к тесту
            someFile = new File("someFile", new byte[] { 1, 2, 3 });
            someFile2 = new File("someFile2", new byte[] { 2, 3, 4 });
            someFile3 = new File("someFile3", new byte[] { 3, 4, 5 });
            signedContent = new byte[] { 1, 7 };
            signedContent3 = new byte[] { 3, 7 };

            cryptographer = A.Fake<ICryptographer>();
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new FileSender(cryptographer, sender, recognizer);
			A.CallTo(() => cryptographer.Sign(someFile.Content, null))
				.WithAnyArguments()
				.Returns(signedContent);
			A.CallTo(() => cryptographer.Sign(someFile3.Content, null))
				.WithAnyArguments()
				.Returns(signedContent3);
			A.CallTo(() => sender.TrySend(null))
				.WithAnyArguments()
				.Returns(true);
		}

		[TestCase("4.0")]
        [TestCase("3.1")]
        public void Send_WhenGoodFormat(string format)
        {
            PrepareDocument(someFile, DateTime.Now, format);

			fileSender.SendFiles(new[] {someFile}, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

		[Test]
		public void Send_WhenYoungerThanAMonth()
		{
			PrepareDocument(someFile,
				DateTime.Now.AddMonths(-1).AddDays(1), "4.0");
			fileSender.SendFiles(new[] { someFile }, certificate)
				.SkippedFiles.Should().BeEmpty();
		}

		[Test]
        public void Skip_WhenBadFormat()
        {
            PrepareDocument(someFile, DateTime.Now, "2.0");

			fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
			A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
				.MustNotHaveHappened();
		}

		[Test]
        public void Skip_WhenOlderThanAMonth()
        {
            PrepareDocument(someFile,
                DateTime.Now.Date.AddMonths(-1).AddDays(-1), "4.0");

            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
			A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
				.MustNotHaveHappened();
		}

		[Test]
        public void Skip_WhenSendFails()
        {
            PrepareDocument(someFile, DateTime.Now, "4.0");
            A.CallTo(() => sender.TrySend(null))
				.WithAnyArguments().Returns(false);

            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
        }

        [Test]
        public void Skip_WhenNotRecognized()
        {
            Document document;
            A.CallTo(() => recognizer.TryRecognize(someFile, out document))
                .Returns(false);
            fileSender.SendFiles(new[] { someFile }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile);
			A.CallTo(() => sender.TrySend(null))
				.WithAnyArguments().MustNotHaveHappened();
		}

		[Test]
        public void IndependentlySend_WhenSeveralFiles()
        {
            //NOTE: Важно задавать, сколько раз должен возвращаться каждый из результатов
            var document1 = PrepareDocument(someFile, DateTime.Now, "4.0");
            var document2 = PrepareDocument(someFile2, DateTime.Now, "2.0");
            var document3 = PrepareDocument(someFile3, DateTime.Now, "4.0");

			A.CallTo(() => cryptographer.Sign(document1.Content, certificate))
				.Returns(signedContent).Once();
			A.CallTo(() => cryptographer.Sign(document3.Content, certificate))
                .Returns(signedContent3).Once();

            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .ReturnsNextFromSequence(false, true);

			fileSender.SendFiles(new[] { someFile, someFile2, someFile3 }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(someFile, someFile2);

			A.CallTo(() => cryptographer.Sign(document2.Content, certificate))
				.MustNotHaveHappened();
		}

		private Document PrepareDocument(File file, DateTime created, string format)
        {
			var document = new Document(file.Name, file.Content, created, format);
			A.CallTo(() => recognizer.TryRecognize(file, out document))
				.Returns(true).Once();
	        return document;
        }
    }
}