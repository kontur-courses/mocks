using System;
using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    [TestFixture]
    public class SingleFileSender_Should
    {
        private SingleFileSender fileSender;
        private IDocumentChecker documentChecker;
        private ICryptographer cryptographer;
        private ISender sender;
        private IRecognizer recognizer;

        private readonly X509Certificate certificate = new X509Certificate();
        private File file;
        private byte[] signedContent;

        [SetUp]
        public void SetUp()
        {
            file = new File("someFile", new byte[] { 1, 2, 3 });
            signedContent = new byte[] { 1, 7 };

            documentChecker = A.Fake<IDocumentChecker>();
            cryptographer = A.Fake<ICryptographer>();
            sender = A.Fake<ISender>();
            recognizer = A.Fake<IRecognizer>();
            fileSender = new SingleFileSender(
                documentChecker, cryptographer,
                sender, recognizer);
        }

        [Test]
        public void Send_WhenGoodDocument()
        {
            var document = BuildDocument(file);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => documentChecker.CheckDocument(document))
                .Returns(true);
            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);
            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(true);

            fileSender.TrySendFile(file, certificate)
                .Should().BeTrue();
        }

        [Test]
        public void NotSend_WhenBadDocument()
        {
            var document = BuildDocument(file);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => documentChecker.CheckDocument(document))
                .Returns(false);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();

            fileSender.TrySendFile(file, certificate)
                .Should().BeFalse();
        }

        [Test]
        public void NotSend_WhenSendFails()
        {
            var document = BuildDocument(file);
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(true);
            A.CallTo(() => cryptographer.Sign(document.Content, certificate))
                .Returns(signedContent);
            A.CallTo(() => sender.TrySend(signedContent))
                .Returns(false);

            fileSender.TrySendFile(file, certificate)
                .Should().BeFalse();
        }

        [Test]
        public void NotSend_WhenNotRecognized()
        {
            Document document;
            A.CallTo(() => recognizer.TryRecognize(file, out document))
                .Returns(false);
            A.CallTo(() => sender.TrySend(A<byte[]>.Ignored))
                .MustNotHaveHappened();

            fileSender.TrySendFile(file, certificate)
                .Should().BeFalse();
        }

        private Document BuildDocument(File file)
        {
            return new Document(file.Name, file.Content, new DateTime(), null);
        }
    }
}