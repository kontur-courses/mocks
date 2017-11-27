using System;
using System.Linq;
using NUnit.Framework;

namespace Challenge.Infrastructure
{
    [TestFixture]
    public class GenerateIncorrectTests
    {
        [Test]
        public void Generate()
        {
            var impls = ChallengeHelpers.GetIncorrectImplementationTypes();
            var code = string.Join(Environment.NewLine,
                impls.Select(imp => $"public class {imp.Name}_Tests : {nameof(IncorrectImplementation_TestsBase)} {{}}")
                );
            Console.WriteLine(code);
        }
    }

    #region Generated with test above

    public class ThingCache0_Should : IncorrectImplementation_TestsBase { }
    public class ThingCache1_Should : IncorrectImplementation_TestsBase { }
    public class ThingCache2_Should : IncorrectImplementation_TestsBase { }
    public class ThingCache3_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheAll_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheCase_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheEmpty_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheNull_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheSingle_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheSingle2_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheTwice_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheRem_Should : IncorrectImplementation_TestsBase { }
    public class ThingCacheSTA_Should : IncorrectImplementation_TestsBase { }

    #endregion
}