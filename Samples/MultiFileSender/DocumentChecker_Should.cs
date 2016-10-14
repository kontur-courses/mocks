using System;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Samples.MultiFileSender.Dependencies;

namespace Samples.MultiFileSender
{
    [TestFixture]
    public class DocumentChecker_Should
    {
        private IDocumentChecker documentChecker;
        private IDateTimeService dateTimeService;
        private readonly DateTime specialDate = new DateTime(2010, 1, 1);

        [SetUp]
        public void SetUp()
        {
            dateTimeService = A.Fake<IDateTimeService>();
            A.CallTo(() => dateTimeService.Now).Returns(specialDate);
            documentChecker = new DocumentChecker(dateTimeService);
        }

        [TestCase("4.0")]
        [TestCase("3.1")]
        public void Pass_WhenGoodFormat(string format)
        {
            documentChecker.CheckDocument(new Document("someFile", null, specialDate, format))
                .Should().BeTrue();
        }

        [Test]
        public void Fail_WhenBadFormat()
        {
            documentChecker.CheckDocument(new Document("someFile", null, specialDate, "2.0"))
                .Should().BeFalse();
        }

        [Test]
        public void Fail_WhenOlderThanAMonth()
        {
            var document = new Document("someFile", null,
                specialDate.Date.AddMonths(-1), "4.0");
            documentChecker.CheckDocument(document)
                .Should().BeFalse();
        }

        [Test]
        public void Pass_WhenYoungerThanAMonth()
        {
            var document = new Document("someFile", null,
                specialDate.Date.AddMonths(-1).AddSeconds(1), "4.0");
            documentChecker.CheckDocument(document)
                .Should().BeTrue();
        }
    }
}