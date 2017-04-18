using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;

namespace Teleopti.Ccc.WinCodeTest.Common.Filter
{
    [TestFixture]
    public class SpecificationFilterTest
    {
        private SpecificationFilter<string> _target;
        private ObservableCollection<string> _collection;
        private FilterEverythingSpecification _filterEverything;
        private FilterNothingSpecification _filterNothing;

        [SetUp]
        public void Setup()
        {
            _filterEverything = new FilterEverythingSpecification();
            _filterNothing = new FilterNothingSpecification();
            _target = new SpecificationFilter<string>();
            _collection = new ObservableCollection<string>();
        }

        [Test]
        public void VerifyCanAddCollectionToFilter()
        {
            _target.FilterCollection(_collection);
            ICollectionView defaultView = CollectionViewSource.GetDefaultView(_collection);
            Assert.IsNotNull(defaultView.Filter);
        }

        [Test]
        public void VerifyFilterIsCalled()
        {
            _target.Filter = _filterEverything;
            _target.FilterCollection(_collection);

            ListCollectionView defaultView = (ListCollectionView)CollectionViewSource.GetDefaultView(_collection);
            Assert.IsFalse(defaultView.Filter.Invoke("something"));
            Assert.IsTrue(_filterEverything.IsCalled);

            _target.Filter = _filterNothing;
            _target.FilterCollection(_collection);
            Assert.IsTrue(defaultView.Filter.Invoke("something"));
            Assert.IsTrue(_filterNothing.IsCalled);
        }


        private class FilterEverythingSpecification : Specification<string>
        {
            public bool IsCalled { get; set; }
            
            public override bool IsSatisfiedBy(string obj)
            {
                IsCalled = true;
                return true;
            }
        }

        private class FilterNothingSpecification : Specification<string>
        {
            public bool IsCalled { get; set; }
            
            public override bool IsSatisfiedBy(string obj)
            {
                IsCalled = true;
                return false;
            }
        }
    }
}
