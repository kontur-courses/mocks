using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
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

            Thing _ = null;
            A.CallTo(() => thingService.TryRead(thingId1, out _))
                .Returns(true)
                .AssignsOutAndRefParameters(thing1);

            A.CallTo(() => thingService.TryRead(thingId2, out _))
                .Returns(true)
                .AssignsOutAndRefParameters(thing2);
        }

        // TODO: Написать простейший тест, а затем все остальные
        // Live Template tt работает!

        // Пример теста
        [Test]
        public void CallThingServiceOnce_OnFirstGet()
        {
            thingCache.Get(thingId1);

            Thing _ = null;

            A.CallTo(() => thingService.TryRead(thingId1, out _)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void CallThingServiceOnce_OnTwoGet()
        {
            thingCache.Get(thingId1);
            thingCache.Get(thingId1);

            Thing _ = null;

            A.CallTo(() => thingService.TryRead(thingId1, out _)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ReturnRightThing_OnGet()
        {
            var thing = thingCache.Get(thingId1);

            thing.Should().Be(thing1);
        }

        [Test]
        public void ReturnNull_OnEmptyId()
        {
            var thing = thingCache.Get("");

            thing.Should().BeNull();
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