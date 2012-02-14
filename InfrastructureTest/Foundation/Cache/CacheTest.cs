using NUnit.Framework;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Cache
{
    public abstract class CacheTest
    {
        protected const string Key = "testet";

        protected ICustomDataCache<CacheTestObject> Target { get; set;}

        protected class CacheTestObject
        {
            public int Value { get; set; }
        }

        [SetUp]
        public void Setup()
        {
            Target = CreateCache<CacheTestObject>();
        }

        protected abstract ICustomDataCache<T> CreateCache<T>();

        [Test]
        public void VerifyCacheWorks()
        {
            var data1 = new CacheTestObject { Value = 17 };
            Target.Put(Key, data1);
            var data2 = Target.Get(Key);
            data1.Value = 123;
            Assert.AreEqual(123, data2.Value);
            Target = CreateCache<CacheTestObject>();
            Assert.AreEqual(123, Target.Get(Key).Value);
        }

        [Test]
        public void VerifyGetPut()
        {
            var data = new CacheTestObject();
            Target.Put(Key, data);
            Assert.AreSame(data, Target.Get(Key));
        }

        [Test]
        public void VerifyUpdate()
        {
            var data = new CacheTestObject();
            var data2 = new CacheTestObject();
            Target.Put(Key, data);
            Target.Put(Key, data2);
            Assert.AreSame(data2, Target.Get(Key));
        }

        [Test]
        public void VerifyUsingSameKeyForDifferentThings()
        {
            var data = new CacheTestObject();
            Target.Put(Key, data);
            ICustomDataCache<int> target2 = CreateCache<int>();
            target2.Put(Key, 3);

            Assert.AreSame(data, Target.Get(Key));
            Assert.AreEqual(3, target2.Get(Key));
        }

        [Test]
        public void VerifyDelete()
        {
            var data = new CacheTestObject();
            Target.Put(Key, data);
            Target.Delete(Key);
            Assert.IsNull(Target.Get(Key));
        }

        [Test]
        public void VerifyDeleteOnNonExistingKey()
        {
            Target.Delete(Key);
            Assert.IsNull(Target.Get(Key));
        }
    }
}
