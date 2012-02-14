using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation.Cache;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Cache
{
    public class DictionaryCacheTest : CacheTest
    {
        protected override ICustomDataCache<T> CreateCache<T>()
        {
            return new DictionaryCache<T>();
        }

        [Test]
        public void VerifyClear()
        {
            DictionaryCache<CacheTestObject> casted = (DictionaryCache<CacheTestObject>)Target;
            Target.Put("a", new CacheTestObject());
            Target.Put("gris", new CacheTestObject());
            casted.Clear();
            Assert.IsNull(Target.Get("a"));
            Assert.IsNull(Target.Get("gris"));
        }

    }
}
