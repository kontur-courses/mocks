using System.Security.Cryptography.X509Certificates;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    [TestFixture]
    public class MultiFileSender_Should
    {
        private MultiFileSender multiFileSender;
        private ISingleFileSender singleFileSender;

        private readonly X509Certificate certificate = new X509Certificate();
        private File file;
        private File file2;
        private File file3;

        [SetUp]
        public void SetUp()
        {
            file = new File("someFile", new byte[] { 1, 2, 3 });
            file2 = new File("someFile2", new byte[] { 2, 3, 4 });
            file3 = new File("someFile3", new byte[] { 3, 4, 5 });

            singleFileSender = A.Fake<ISingleFileSender>();
            multiFileSender = new MultiFileSender(singleFileSender);
        }

        [Test]
        public void Send_WhenSingle()
        {
            A.CallTo(() => singleFileSender.TrySendFile(file, certificate))
                .Returns(true);

            multiFileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEmpty();
        }

        [Test]
        public void Skip_WhenSingle()
        {
            A.CallTo(() => singleFileSender.TrySendFile(file, certificate))
                .Returns(false);

            multiFileSender.SendFiles(new[] { file }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file);
        }

        [Test]
        public void IndependentlySend_WhenSeveralFiles()
        {
            A.CallTo(() => singleFileSender.TrySendFile(file, certificate))
                .Returns(true);
            A.CallTo(() => singleFileSender.TrySendFile(file2, certificate))
                .Returns(false);
            A.CallTo(() => singleFileSender.TrySendFile(file3, certificate))
                .Returns(true);

            multiFileSender.SendFiles(new[] { file, file2, file3 }, certificate)
                .SkippedFiles.Should().BeEquivalentTo(file2);
        }
    }
}