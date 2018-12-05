using System;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Samples
{
    public class Dto
    {
        public string s;
    }

    public interface IService
    {
        string Get();
    }

    [TestFixture]
    public class FakeItEasy_Should
    {
        [Test]
        public void Fail_OnNotConfiguredCalls_InStrictMode()
        {
            var service = A.Fake<IService>(o => o.Strict());
            Assert.Throws<ExpectationException>(
                () => service.Get());

        }
        [Test]
        public void ReturnsDefault_AfterSequenceEnds()
        {
            var service = A.Fake<IService>();
            A.CallTo(() => service.Get())
                .ReturnsNextFromSequence("1", "2");
            service.Get();
            service.Get();
            service.Get().Should().Be("");
        }

        public class ComplexDto
        {
            public readonly Dto dto;
            public readonly string s;

            public ComplexDto()
            {
            }

            public ComplexDto(Dto dto)
            {
                this.dto = dto;
                s = "Created with complex constructor";
            }
        }

        [Test]
        public void Creates_ObjectWithParameterlessConstructor()
        {
            var func = A.Fake<Func<ComplexDto>>();
            var complexDto = func();
            complexDto.Should().NotBeNull();
            complexDto.dto.Should().BeNull();
            complexDto.s.Should().BeNull();
        }

        [Test]
        public void ReturnsOnce_HasStackBehaviour()
        {
            var service = A.Fake<IService>();
            A.CallTo(() => service.Get()).Returns("1").Once();
            A.CallTo(() => service.Get()).Returns("2").Once();
            service.Get().Should().Be("2");
            A.CallTo(() => service.Get()).Returns("3").Once();
            service.Get().Should().Be("3");
            service.Get().Should().Be("1");
            service.Get().Should().Be("");
        }

    }
}