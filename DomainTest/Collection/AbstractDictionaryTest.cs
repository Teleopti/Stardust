using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.DomainTest.Collection
{
    //really stupid coverage tests... just testing MS dic really...
    [TestFixture]
    public class AbstractDictionaryTest
    {
        private IDictionary<int, string> target;

        [SetUp]
        public void Setup()
        {
            var dic = new Dictionary<int, string>();
            dic[1] = "ett";
            dic[2] = "två";
            target = new targetDic(dic);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item"), Test]
        public void VerifyEnumerators()
        {
            int count = 0;
            foreach (var item in target)
            {
                count++;
            }
            Assert.AreEqual(2, count);
            count = 0;
            foreach (var item in (IEnumerable)target)
            {
                count++;
            }
            Assert.AreEqual(2, count);
        }

        [Test]
        public void VerifyCopyTo()
        {
            var arr=new KeyValuePair<int, string>[target.Count];
            target.CopyTo(arr, 0);
            Assert.AreEqual(2, arr.Length);
        }

        [Test]
        public void VerifyAdd()
        {
            target.Add(3, "tre");
            Assert.AreEqual(target[3], "tre");
            target.Add(new KeyValuePair<int, string>(17, "sjutton"));
            Assert.AreEqual(4, target.Count);
        }

        [Test]
        public void VerifyItem()
        {
            target[100] = "hundra";
            Assert.AreEqual("hundra", target[100]);
            string s;
            Assert.IsTrue(target.TryGetValue(100, out s));
        }

        [Test]
        public void VerifyRemove()
        {
            target.Remove(2);
            Assert.AreEqual(1, target.Count);
            Assert.IsFalse(target.Remove(new KeyValuePair<int, string>(234, "dfg")));
            target.Clear();
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void VerifyKeysAndValues()
        {
            Assert.AreEqual(target.Keys.Count, target.Values.Count);
        }

        [Test]
        public void VerifyContains()
        {
            Assert.IsTrue(target.Contains(new KeyValuePair<int, string>(2, "två")));
            Assert.IsFalse(target.ContainsKey(7));
        }

        [Test]
        public void VerifyReadOnly()
        {
            Assert.IsFalse(target.IsReadOnly);
        }


        private class targetDic : AbstractDictionary<int, string>
        {
            public targetDic(IDictionary<int, string> dictionary) : base(dictionary)
            {
            }
        }
    }
}
