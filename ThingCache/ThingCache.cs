using System.Collections.Generic;
using NUnit.Framework;

namespace MockFramework
{
    public class ThingCache : IThingCache
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
        private IThingCache thingCache;

        private const string thingId1 = "TheDress";
        private Thing thing1 = new Thing(thingId1);

        private const string thingId2 = "CoolBoots";
        private Thing thing2 = new Thing(thingId2);

        public static string Authors = "<ВАШИ ФАМИЛИИ>"; // e.g. "Zharkov Peshkov"

        public virtual IThingCache CreateThingCache(IThingService thingService)
        {
            // переопределяется при запуске exe
            return new ThingCache(thingService);
        }

        [SetUp]
        public void SetUp()
        {
            //thingService = A...
            thingCache = CreateThingCache(thingService);
        }

        //TODO: написать простейший тест, а затем все остальные
        //Live Template tt работает!
    }
}