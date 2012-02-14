using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Tests for ObjectPair class
    /// </summary>
    [TestFixture]
    public class ObjectPairTest
    {
        /// <summary>
        /// Determines whether this instance [can create object pair].
        /// </summary>
        [Test]
        public void CanCreateObjectPair()
        {
            ObjectPair<int, int> pair = new ObjectPair<int, int>(7, 3);
            Assert.AreEqual(7, pair.Obj1);
            Assert.AreEqual(3, pair.Obj2);
        }
    }
}