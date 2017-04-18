using System.Windows.Data;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    [TestFixture]
    public class ShowOnlyCollectionTest
    {
        private ShowOnlyCollection<object> _target;

        [SetUp]
        public void Setup()
        {
            _target = new ShowOnlyCollection<object>();
        }

        [Test]
        public void VerifyOnlyShowsItemsDefinedInFilter()
        {
            string str1 = "one";
            bool bool1 = true;
            int int1 = 1;
      

            _target.Add(str1);
            _target.Add(bool1);
            _target.Add(int1);

            _target.AddFilter(typeof(int));
            Assert.IsFalse(CollectionViewSource.GetDefaultView(_target).Contains(str1));
            Assert.IsFalse(CollectionViewSource.GetDefaultView(_target).Contains(bool1));
            Assert.IsTrue(CollectionViewSource.GetDefaultView(_target).Contains(int1));

            _target.AddFilter(typeof(bool));
            Assert.IsTrue(CollectionViewSource.GetDefaultView(_target).Contains(bool1));
            Assert.IsTrue(CollectionViewSource.GetDefaultView(_target).Contains(int1));

        }
    }
}
