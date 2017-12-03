using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MockFramework
{
	[TestFixture]
	public class ThingCache_Tests
	{
		private IThingService thingService;
		private IThingCache thingCache;

		private const string thingId1 = "TheDress";
		private Thing thing1 = new Thing(thingId1);

		private const string thingId2 = "CoolBoots";
		private Thing thing2 = new Thing(thingId2);

		public static string Authors = "<ВАШИ ФАМИЛИИ>"; // e.g. "Zharkov Peshkov"

		public virtual IThingCache CreateThingCache(IThingService thingService)
		{
			return new ThingCache(thingService);
		}

		[SetUp]
		public void SetUp()
		{
			thingService = A.Fake<IThingService>();
			thingCache = CreateThingCache(thingService);
		}

		[Test]
		public void Get_NonexistingObject_ReturnsNull()
		{
			thingCache.Get(thingId1)
				.Should().BeNull();
			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.MustHaveHappened();
		}

		[Test]
		public void Get_ExistingThingFirstTime_TakeItFromService()
		{
			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.Returns(true);

			thingCache.Get(thingId1)
				.Should().Be(thing1);
		}

		[Test]
		public void Get_OneThingTwice_CallsServiceOnce()
		{
			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.Returns(true)
				.AssignsOutAndRefParameters(thing1);

			thingCache.Get(thingId1);
			thingCache.Get(thingId1);

			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.MustHaveHappened(Repeated.Exactly.Once);
		}

		[Test]
		public void Get_ReturnsDifferentThings_ForDifferentIds()
		{
			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.Returns(true);
			A.CallTo(() => thingService.TryRead(thingId2, out thing2))
				.Returns(true);

			thingCache.Get(thingId1).Should().Be(thing1);
			thingCache.Get(thingId2).Should().Be(thing2);
		}

		[Test]
		public void Get_DontCacheNulls()
		{
			thingCache.Get(thingId1);
			thingCache.Get(thingId1);

			A.CallTo(() => thingService.TryRead(thingId1, out thing1))
				.MustHaveHappened(Repeated.Exactly.Twice);

		}

	}
}