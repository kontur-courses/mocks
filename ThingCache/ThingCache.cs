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

        private const string thingIdNotExisted = "NotExisted";
        private Thing thing3 = null;

        // Метод, помеченный атрибутом SetUp, выполняется перед каждым тестов
        [SetUp]
        public void SetUp()
        {
            thingService = A.Fake<IThingService>();
            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).Returns(true);
            A.CallTo(() => thingService.TryRead(thingId2, out thing2)).Returns(true);
            A.CallTo(() => thingService.TryRead(thingIdNotExisted, out thing3)).Returns(false);
            thingCache = new ThingCache(thingService);
        }

        // TODO: Написать простейший тест, а затем все остальные
        // Live Template tt работает!

        // Пример теста
        [Test]
        public void ThingCache_Get_ReturnsCorrectValue()
        {
            var actualValue = thingCache.Get(thingId1);
            Assert.That(actualValue, Is.EqualTo(thing1));
        }

        [Test]
        public void ThingCache_Get_ReturnsNoValue()
        {
            var notExistedValue = thingCache.Get(thingIdNotExisted);
            Assert.That(notExistedValue, Is.Null);
        }

        [Test]
        public void ThingCache_Get_MethodCallingHappened()
        {
            thingCache.Get(thingId1);
            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ThingCache_Get_ReturnsValueFromCache()
        {
            thingCache.Get(thingId1); // put value to cache
            thingCache.Get(thingId1); // get value via cache
            A.CallTo(() => thingService.TryRead(thingId1, out thing1)).MustHaveHappenedOnceExactly();
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