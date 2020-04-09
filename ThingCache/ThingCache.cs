using System.Collections.Generic;

namespace MockFramework
{
    public class ThingCache
    {
        private readonly IDictionary<string, Thing> dictionary = new Dictionary<string, Thing>();
        private readonly IThingService thingService;

        public ThingCache(IThingService thingService)
        {
            this.thingService = thingService;
        }

        public Thing Get(string thingId)
        {
            if (dictionary.TryGetValue(thingId, out var thing)) return thing;
            if (!thingService.TryRead(thingId, out thing)) return null;

            dictionary[thingId] = thing;
            
            return thing;
        }
    }
}