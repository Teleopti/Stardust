using System.Collections;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using System.Collections.Generic;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Tests for the ObjectPairCollection class
    /// </summary>
    [TestFixture]
    public class ObjectPairCollectionTest
    {
        /// <summary>
        /// Determines whether this instance [can set and retriew object and handle not found].
        /// </summary>
        [Test]
        public void CanSetAndRetrieveObjectAndHandleNotFound()
        {
            ObjectPairCollection<oldClass, newClass> pairList = new ObjectPairCollection<oldClass, newClass>();
            oldClass old1 = new oldClass(7);
            oldClass old2 = new oldClass(9);
            oldClass old3 = new oldClass(-1);
            newClass new1 = new newClass(77);
            newClass new2 = new newClass(99);
            pairList.Add(old1, new1);
            pairList.Add(old2, new2);
            Assert.AreEqual(new1, pairList.GetPaired(old1));
            Assert.IsNull(pairList.GetPaired(old3));
        }

        /// <summary>
        /// Check that it's possible to iterate over collection
        /// </summary>
        [Test]
        public void CanIterateOverCollection()
        {
            ObjectPairCollection<oldClass, newClass> pairList = new ObjectPairCollection<oldClass, newClass>();
            oldClass old1 = new oldClass(7);
            oldClass old2 = new oldClass(9);
            newClass new1 = new newClass(77);
            newClass new2 = new newClass(99);
            pairList.Add(old1, new1);
            pairList.Add(old2, new2);

            foreach (ObjectPair<oldClass, newClass> pair in pairList)
            {
                Assert.IsNotNull(pair.Obj1);
                Assert.IsNotNull(pair.Obj2);
            }

            IEnumerator localEnumerator = pairList.GetEnumerator();
            Assert.IsNotNull(localEnumerator);

        }



        [Test]
        public void VerifyGetObj2Collection()
        {
            ObjectPairCollection<int, int> pairList = new ObjectPairCollection<int, int>();
            pairList.Add(1, 3);
            pairList.Add(74,1);
            ICollection<int> obj2Collection = pairList.Obj2Collection();
            Assert.AreEqual(2, obj2Collection.Count);
            Assert.IsTrue(obj2Collection.Contains(3));
            Assert.IsTrue(obj2Collection.Contains(1));
        }

        [Test]
        public void VerifyGetObj1Collection()
        {
            ObjectPairCollection<int, int> pairList = new ObjectPairCollection<int, int>();
            pairList.Add(1, 3);
            pairList.Add(74, 1);
            ICollection<int> obj2Collection = pairList.Obj1Collection();
            Assert.AreEqual(2, obj2Collection.Count);
            Assert.IsTrue(obj2Collection.Contains(74));
            Assert.IsTrue(obj2Collection.Contains(1));
        }

        private class oldClass
        {
            private int _id;

            public oldClass(int id)
            {
                _id = id;
            }
        }

        private class newClass
        {
            private int _id;

            public newClass(int id)
            {
                _id = id;
            }
        }
    }
}