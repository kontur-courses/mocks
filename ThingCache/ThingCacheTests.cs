using System;
using FakeItEasy;
using NUnit.Framework;

namespace MockFramework
{
    [TestFixture]
    public class ThingCacheTests
    {
        private IThingService thingService;
        private ThingCache thingCache;

        private const string thingId1 = "TheDress";
        private static Thing thing1 = new Thing(thingId1);

        private const string thingId2 = "CoolBoots";
        private static Thing thing2 = new Thing(thingId2);

        private static object[] ValidIdentifiers =
        {
            new object[] {thingId1, thing1},
            new object[] {thingId2, thing2}
        };

        private static object[] WrongIdentifiers =
        {
            new object[] {""},
            new object[] {"someID"}
        };

        [SetUp]
        public void SetUp()
        {
            thingService = A.Fake<IThingService>();

            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).Returns(true);
            A.CallTo(() => thingService.TryRead(thingId2, out thing2)).Returns(true);

            thingCache = new ThingCache(thingService);
        }

        [Test, TestCaseSource(nameof(WrongIdentifiers))]
        public void Get_ReturnsNullByUnknownId(string thingId)
        {
            var t = thingCache.Get(thingId);
            Assert.That(t, Is.Null);
        }
        
        [Test]
        public void Get_ThrowsExceptionByBadId()
        {
            var exception = Assert.Catch<Exception>(() => thingCache.Get(null));
            Assert.That(exception, Is.TypeOf<ArgumentNullException>());
        }

        [Test, TestCaseSource(nameof(ValidIdentifiers))]
        public void Get_ReturnsThingByThingId(string thingId, Thing thing)
        {
            var t = thingCache.Get(thingId);
            Assert.That(t, Is.EqualTo(thing));
        }

        [Test]
        public void Get_CallsTryReadForFirstTry()
        {
            thingCache.Get(thingId1);
            Thing t;
            A.CallTo(() => thingService.TryRead(A<string>.Ignored, out t)).MustHaveHappened();
        }

        [TestCase(thingId1, 10)]
        public void Get_CallsOnceTryReadForOthers(string thingId, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                thingCache.Get(thingId);
            }
            
            Thing t;
            A.CallTo(() => thingService.TryRead(thingId1, out t)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestCase(thingId1)]
        public void Get_ReturnsFromCacheForOthers(string thingId)
        {
            var firstThing = thingCache.Get(thingId);
            var secondThing = thingCache.Get(thingId);
            
            Assert.That(firstThing, Is.EqualTo(secondThing));
        }
        
        [Test]
        public void Get_ReturnsFromCacheForDifferentThings()
        {
            var firstThing = thingCache.Get(thingId1);
            var secondThing = thingCache.Get(thingId2);
            
            Assert.That(firstThing, Is.Not.EqualTo(secondThing));
        }
    }
}