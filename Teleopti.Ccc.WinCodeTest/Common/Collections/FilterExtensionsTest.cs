using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    [TestFixture]
    public class FilterExtensionsTest
    {
        private ObservableCollection<int> _target;
        private int[] _underFive;
        private int[] _fiveAndOver;

        [SetUp]
        public void Setup()
        {
            _underFive= new int[] {1,2,3,4,5};
            _fiveAndOver = new int[] { 5,6,7,8,9,10 };
            _target = new ObservableCollection<int>(_underFive.Concat(_fiveAndOver).ToList());
        }

        [Test]
        public void VerifyCanFilterTheDefaultView()
        {
            SimpleSpecification spec = new SimpleSpecification(_underFive);
            _target.FilterOutBySpecification(spec);
            ListCollectionView defaultView = (ListCollectionView) CollectionViewSource.GetDefaultView(_target);
            CheckContains(defaultView, _fiveAndOver, _underFive);
        }

        [Test]
        public void VerifyCanCreateAViewThatIsFiltered()
        {
            SimpleSpecification spec = new SimpleSpecification(_fiveAndOver);
            ListCollectionView createdView = _target.CreateFilteredView(spec);
            CheckContains(createdView,_underFive, _fiveAndOver);
        }

        //Checks all the items in the view and checks that they are in the contained but not the filtered
        private static void CheckContains(ListCollectionView view,IEnumerable<int> contained,IEnumerable<int> filtered)
        {
            foreach (int item in view)
            {
                Assert.IsTrue(contained.Contains(item));
                Assert.IsFalse(filtered.Contains(item));
            }
        }

        private class SimpleSpecification : Specification<int>
        {
            public IList<int> ApprovedInts { get; }

            public SimpleSpecification(IEnumerable<int> approved)
            {
                ApprovedInts = new List<int>(approved);
            }
            public override bool IsSatisfiedBy(int obj)
            {
                return ApprovedInts.Contains(obj);
            }
        }
    }
}
