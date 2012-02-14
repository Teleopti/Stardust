using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    public class CollectionHelperTest
    {
        [Test]
        public void VerifyToDistinctGenericCollection()
        {
            IList<int> theInts = new List<int> {1, 2};

            ICollection<int> intsInCollection =
                Infrastructure.Repositories.CollectionHelper.ToDistinctGenericCollection<int>(theInts);
            Assert.IsNotNull(intsInCollection);
            Assert.AreEqual(2, intsInCollection.Count);
            Type type = intsInCollection.GetType().GetInterface("System.Collections.Generic.ICollection`1", true);
            Assert.IsNotNull(type);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyToDistinctGenericCollectionFailsWithNonIList()
        {
            int anInt = 3;
            Infrastructure.Repositories.CollectionHelper.ToDistinctGenericCollection<int>(anInt);
        }
    }
}
