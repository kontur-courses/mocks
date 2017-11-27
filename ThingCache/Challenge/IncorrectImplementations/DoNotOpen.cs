using System;
using System.Collections.Generic;
using System.Linq;
using Challenge.Infrastructure;
using MockFramework;

namespace Challenge.IncorrectImplementations
{
    #region Не подглядывать!

    // [IncorrectImplementation]
    public abstract class ThingCacheBase : IThingCache
    {
        protected readonly IThingService thingService;
        protected IDictionary<string, Thing> dictionary = new Dictionary<string, Thing>();

        protected ThingCacheBase(IThingService thingService)
        {
            this.thingService = thingService;
        }

        public virtual Thing Get(string thingId)
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
    
    public abstract class ThingCacheWithCapacity : ThingCacheBase
    {
        protected abstract int CacheSize { get; }
        private readonly Queue<KeyValuePair<string, Thing>> cache = new Queue<KeyValuePair<string, Thing>>();
        
        public ThingCacheWithCapacity(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            var kvp = cache.FirstOrDefault(x => x.Key == thingId);
            if (kvp.Value != null)
                return kvp.Value;
            
            Thing thing;
            if (!thingService.TryRead(thingId, out thing) || thing == null)
                return null;
            cache.Enqueue(new KeyValuePair<string, Thing>(thingId, thing));
            if (cache.Count > CacheSize)
                cache.Dequeue();
            return thing;
        }
    }
    
    [IncorrectImplementation("Ничего не кэширует")]
    public class ThingCache0 : ThingCacheWithCapacity
    {
        public ThingCache0(IThingService thingService)
            : base(thingService)
        {
        }

        protected override int CacheSize { get; } = 0;
    }
    
    [IncorrectImplementation("Размер кэша - 1 элемент")]
    public class ThingCache1 : ThingCacheWithCapacity
    {
        public ThingCache1(IThingService thingService)
            : base(thingService)
        {
        }

        protected override int CacheSize { get; } = 1;
    }
    
    [IncorrectImplementation("Размер кэша - 2 элемента")]
    public class ThingCache2 : ThingCacheWithCapacity
    {
        public ThingCache2(IThingService thingService)
            : base(thingService)
        {
        }

        protected override int CacheSize { get; } = 2;
    }

    [IncorrectImplementation("Размер кэша - 3 элемента")]
    public class ThingCache3 : ThingCacheWithCapacity
    {
        public ThingCache3(IThingService thingService)
            : base(thingService)
        {
        }

        protected override int CacheSize { get; } = 3;
    }
    
    [IncorrectImplementation("Не проверяет, вернул ли ThingService осмысленный результат")]
    public class ThingCacheAll : ThingCacheBase
    {
        public ThingCacheAll(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            thingService.TryRead(thingId, out thing);
            dictionary[thingId] = thing;
            return thing;
        }
    }
    
    [IncorrectImplementation("Не учитывает регистр ключей")]
    public class ThingCacheCase : ThingCacheBase
    {
        public ThingCacheCase(IThingService thingService)
            : base(thingService)
        {
            dictionary = new Dictionary<string, Thing>(StringComparer.OrdinalIgnoreCase);
        }
    }
    
    [IncorrectImplementation("Возвращает Thing с пустым именем вместо null")]
    public class ThingCacheEmpty : ThingCacheBase
    {
        private new readonly IDictionary<string, Thing> dictionary
            = new Dictionary<string, Thing>();

        public ThingCacheEmpty(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            if (thingService.TryRead(thingId, out thing))
            {
                dictionary[thingId] = thing;
                return thing;
            }
            return new Thing(string.Empty);
        }
    }
    
    [IncorrectImplementation("Возвращает Thing с именем null вместо Thing == null")]
    public class ThingCacheNull : ThingCacheBase
    {
        private new readonly IDictionary<string, Thing> dictionary
            = new Dictionary<string, Thing>();

        public ThingCacheNull(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            if (thingService.TryRead(thingId, out thing))
            {
                dictionary[thingId] = thing;
                return thing;
            }
            return new Thing(string.Empty);
        }
    }
    
    [IncorrectImplementation("Кэширует одно (первое) значение для всех")]
    public class ThingCacheSingle : ThingCacheBase
    {
        private Thing cachedThing;

        public ThingCacheSingle(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (cachedThing != null)
                return cachedThing;
            if (thingService.TryRead(thingId, out thing))
            {
                cachedThing = thing;
                return thing;
            }
            return null;
        }
    }
    
    [IncorrectImplementation("Кэширует одно (последнее) значение для всех")]
    public class ThingCacheSingle2 : ThingCacheBase
    {
        private Thing cachedThing;
        private HashSet<string> cachedKeys = new HashSet<string>();

        public ThingCacheSingle2(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (cachedKeys.Contains(thingId) && cachedThing != null)
                return cachedThing;
            if (thingService.TryRead(thingId, out thing))
            {
                cachedKeys.Add(thingId);
                cachedThing = thing;
                return thing;
            }
            return null;
        }
    }
    
    [IncorrectImplementation("Дважды запрашивает thing у ThingService")]
    public class ThingCacheTwice : ThingCacheBase
    {
        public ThingCacheTwice(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            if (thingService.TryRead(thingId, out thing))
            {
                thingService.TryRead(thingId, out thing);
                dictionary[thingId] = thing;
                return thing;
            }
            return null;
        }
    }
    
    [IncorrectImplementation("Удаляет элементы из кэша при обращении")]
    public class ThingCacheRem : ThingCacheBase
    {
        public ThingCacheRem(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
        {
            var remove = dictionary.ContainsKey(thingId);
            var thing = base.Get(thingId);
            if (remove)
                dictionary.Remove(thingId);
            return thing;
        }
    }
    
    [IncorrectImplementation("Статический кэш")]
    public class ThingCacheSTA : ThingCacheBase
    {
        private new static readonly IDictionary<string, Thing> dictionary
            = new Dictionary<string, Thing>();

        public ThingCacheSTA(IThingService thingService)
            : base(thingService)
        {
        }

        public override Thing Get(string thingId)
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
    

    #endregion
}