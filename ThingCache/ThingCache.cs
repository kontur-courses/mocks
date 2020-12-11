using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;

namespace MockFramework
{
    public class ThingCache
    {
        private readonly IDictionary<string, Thing> dictionary
            = new Dictionary<string, Thing>();
        private readonly IThingService thingService;

        public ThingCache(IThingService thingService)
        {
            this.thingService = thingService;
        }

        public Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            if (thingService.TryRead(thingId, out thing))
            {
                dictionary[thingId] = thing;
                return thing;
            }
            return null;
        }
    }

    [TestFixture]
    public class ThingCache_Should
    {
        private IThingService thingService;
        private ThingCache thingCache;

        private const string thingId1 = "TheDress";
        private Thing thing1 = new Thing(thingId1);

        private const string thingId2 = "CoolBoots";
        private Thing thing2 = new Thing(thingId2);

        // Метод, помеченный атрибутом SetUp, выполняется перед каждым тестов
        [SetUp]
        public void SetUp()
        {
            thingService = A.Fake<IThingService>();

            thingCache = new ThingCache(thingService);
        }

        // TODO: Написать простейший тест, а затем все остальные
        // Live Template tt работает!

        // Пример теста
        [Test]
        public void GetThingAndReturn()
        {
            A.CallTo(() => thingService.TryRead(thingId1, out thing1))
                .Returns(true);

            var actualThing = thingCache.Get(thingId1);

            Assert.AreEqual(thingId1, actualThing.ThingId);
        }

        [Test]
        public void GetNotExistThing()
        {
            Thing thing;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .Returns(false);

            var actualThing = thingCache.Get(thingId1);

            Assert.AreEqual(null, actualThing);
        }

        [Test]
        public void GetNotExistThingTwice()
        {
            Thing thing;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .Returns(false);

            thingCache.Get(thingId1);
            var secondThing1 = thingCache.Get(thingId1);

            Assert.AreEqual(null, secondThing1);
        }

        [Test]
        public void GetNotExistThingTwiceMore()
        {
            Thing thing;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .Returns(false);

            thingCache.Get(thingId1);
            thingCache.Get(thingId1);

            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void GetExistThingTwice()
        {
            A.CallTo(() => thingService.TryRead(thingId1, out thing1))
                .Returns(true);

            var firstCallThing = thingCache.Get(thingId1);
            var secondCallThing = thingCache.Get(thingId1);

            Assert.AreEqual(thingId1, firstCallThing.ThingId);
            Assert.AreEqual(thingId1, secondCallThing.ThingId);
            Thing thing;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void GetExistSeveralThings()
        {
            A.CallTo(() => thingService.TryRead(thingId1, out thing1))
                .Returns(true);
            A.CallTo(() => thingService.TryRead(thingId2, out thing2))
                .Returns(true);

            thingCache.Get(thingId1);
            thingCache.Get(thingId2);
            var secondCallThing1 = thingCache.Get(thingId1);
            var secondCallThing2 = thingCache.Get(thingId2);

            Assert.AreEqual(thingId1, secondCallThing1.ThingId);
            Assert.AreEqual(thingId2, secondCallThing2.ThingId);

            Thing thing;
            A.CallTo(() => thingService.TryRead(thingId1, out thing))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => thingService.TryRead(thingId2, out thing))
                .MustHaveHappenedOnceExactly();
        }

        /** Проверки в тестах
         * Assert.AreEqual(expectedValue, actualValue);
         * actualValue.Should().Be(expectedValue);
         */

        /** Синтаксис AAA
         * Arrange:
         * var fake = A.Fake<ISomeService>();
         * A.CallTo(() => fake.SomeMethod(...)).Returns(true);
         * Assert:
         * var value = "42";
         * A.CallTo(() => fake.TryRead(id, out value)).MustHaveHappened();
         */


        /** Синтаксис out
         * var value = "42";
         * string _;
         * A.CallTo(() => fake.TryRead(id, out _)).Returns(true)
         *     .AssignsOutAndRefParameters(value);
         * A.CallTo(() => fake.TryRead(id, out value)).Returns(true);
         */

        /** Синтаксис Repeat
         * var value = "42";
         * A.CallTo(() => fake.TryRead(id, out value))
         *     .MustHaveHappened(Repeated.Exactly.Twice)
         */
    }
}