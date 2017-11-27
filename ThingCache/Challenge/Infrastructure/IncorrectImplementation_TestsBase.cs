using System;
using Challenge.IncorrectImplementations;
using MockFramework;
using NUnit.Framework;

namespace Challenge.Infrastructure
{
    public abstract class IncorrectImplementation_TestsBase : ThingCache_Should
    {
        public override IThingCache CreateThingCache(IThingService thingService)
        {
            string ns = typeof(ThingCacheBase).Namespace;
            var implTypeName = ns + "." + GetType().Name.Replace("_Should", "");
            var implType = GetType().Assembly.GetType(implTypeName);
            if (implType == null)
                Assert.Fail("no type {0}", implTypeName);
            return (IThingCache)Activator.CreateInstance(implType, thingService);
        }
    }
}

