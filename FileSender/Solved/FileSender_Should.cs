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

			A.CallTo(() => cryptographer.Sign(null, null))
				.WithAnyArguments()
				.Returns(Guid.NewGuid().ToByteArray());
			A.CallTo(() => sender.TrySend(null))
				.WithAnyArguments()
				.Returns(true);
		}

		[TestCase("4.0")]
		[TestCase("3.1")]
		public void Send_WhenGoodFormat(string format)
		{
			var someFile = PrepareDocument(DateTime.Now, format);

			AssertSentSuccessful(someFile);
		}

		[Test]
		public void Send_WhenYoungerThanAMonth()
		{
			var almostMonthAgo = DateTime.Now.AddMonths(-1).AddDays(1);
			var someFile = PrepareDocument(almostMonthAgo);
			AssertSentSuccessful(someFile);
		}

		[Test]
		public void Skip_WhenBadFormat()
		{
			var someFile = PrepareDocument(DateTime.Now, "2.0");
			AssertCanNotBeSent(someFile);
		}

		[Test]
		public void Skip_WhenOlderThanAMonth()
		{
			var someFile = PrepareDocument(
				DateTime.Now.Date.AddMonths(-1).AddDays(-1));

			AssertCanNotBeSent(someFile);
		}

		[Test]
		public void Skip_WhenSendFails()
		{
			var someFile = PrepareSomeGoodDocument();
			A.CallTo(() => sender.TrySend(null))
				.WithAnyArguments().Returns(false);

			fileSender.SendFiles(new[] { someFile }, certificate)
				.SkippedFiles.Should().BeEquivalentTo(someFile);
		}

		[Test]
		public void Skip_WhenNotRecognized()
		{
			var file = PrepareSomeGoodDocument();
			Document document;
			A.CallTo(() => recognizer.TryRecognize(file, out document))
				.Returns(false);
			AssertCanNotBeSent(file);
		}

		[Test]
		public void IndependentlySend_WhenSeveralFiles()
		{
			var file1 = PrepareSomeGoodDocument();
			var file2 = PrepareSomeGoodDocument();
			var file3 = PrepareSomeGoodDocument();

			A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
				.ReturnsNextFromSequence(false, true);

			fileSender.SendFiles(new[] { file1, file2, file3 }, certificate)
				.SkippedFiles.Should().BeEquivalentTo(file1, file2);

			A.CallTo(() => cryptographer.Sign(file2.Content, certificate))
				.MustNotHaveHappened();
		}

		private File PrepareSomeGoodDocument()
		{
			return PrepareDocument(DateTime.Now);
		}

		private File PrepareDocument(DateTime created, string format = "4.0")
		{
			var file = new File(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToByteArray());
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