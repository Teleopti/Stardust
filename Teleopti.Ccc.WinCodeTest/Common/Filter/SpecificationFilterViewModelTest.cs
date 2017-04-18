using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;

namespace Teleopti.Ccc.WinCodeTest.Common.Filter
{
    [TestFixture]
    public class SpecificationFilterViewModelTest
    {
        private SpecificationFilterViewModel<int> _target;
        private TesterForCommandModels _testerForCommandModels;
        private IList<int> _targetCollection;
        private IList<int> _evenNumbers;
        private IList<int> _oddNumbers;
        private Specification<int> _specification;
        
            
        [SetUp]
        public void Setup()
        {
            _specification = new EvenNumberSpecification();
            _targetCollection = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _evenNumbers = _targetCollection.Where(IsEven).ToList();
            _oddNumbers = _targetCollection.Where(n => !IsEven(n)).ToList();
            _testerForCommandModels = new TesterForCommandModels();
            _target = new SpecificationFilterViewModel<int>(_targetCollection, _specification);
            
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.IsFalse(_target.FilterIsActive);
            Assert.IsFalse(_target.IsSelected);
            Assert.IsFalse(_target.ShowOnlyBySpecification);
        }

        [Test]
        public void VerifyFilterOnOffCommand()
        {
            Assert.IsTrue(CollectionViewComparer<int>.CreateDefaultComparer(_targetCollection).ContainsOnlyElementsFrom(_targetCollection),"Default view is not filtered");
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.FilterOnOffCommand));

            _testerForCommandModels.ExecuteCommandModel(_target.FilterOnOffCommand);
            Assert.IsTrue(_target.FilterIsActive);
            Assert.IsTrue(CollectionViewComparer<int>.CreateDefaultComparer(_targetCollection).ContainsOnlyElementsFrom(_oddNumbers),"Default view shows only items not satisfied by the specification");

            //Make sure the totalview is not filtered:
            Assert.IsTrue(CollectionViewComparer<int>.CreateComparer(_target.TotalView).ContainsElements(_targetCollection));

            _testerForCommandModels.ExecuteCommandModel(_target.FilterOnOffCommand);
            Assert.IsTrue(CollectionViewComparer<int>.CreateDefaultComparer(_targetCollection).ContainsOnlyElementsFrom(_targetCollection), "Default view is reset");
            Assert.IsFalse(_target.FilterIsActive);
        }

        [Test]
        public void VerifyShowOnlyReverseFilter()
        {
            _testerForCommandModels.ExecuteCommandModel(_target.FilterOnOffCommand);
            _target.ShowOnlyBySpecification = true;
            Assert.IsTrue(_target.FilterIsActive,"Filter is still active");
            Assert.IsTrue(CollectionViewComparer<int>.CreateDefaultComparer(_targetCollection).ContainsOnlyElementsFrom(_evenNumbers), "Default view shows only items not satisfied by the specification");

        }

        [Test]
        public void VerifyFilterSelectedOnOffCommand()
        {
            Assert.AreEqual(UserTexts.Resources.Filter, _target.FilterSelectedOnOffCommand.Text);
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.FilterSelectedOnOffCommand));
            _testerForCommandModels.ExecuteCommandModel(_target.FilterSelectedOnOffCommand);
            Assert.IsFalse(_target.FilterIsActive);
            _target.IsSelected = true;
            _testerForCommandModels.ExecuteCommandModel(_target.FilterSelectedOnOffCommand);
        }

        [Test]
        public void VerifyFilterView()
        {
            //The viewmodel has a "preview" thats always filtered (or show only)....
            Assert.IsTrue(CollectionViewComparer<int>.CreateComparer(_target.FilteredOutView).ContainsOnlyElementsFrom(_oddNumbers), "The view is filtered from items that are satisfied from the specification");
            Assert.IsTrue(CollectionViewComparer<int>.CreateComparer(_target.ShowOnlyView).ContainsOnlyElementsFrom(_evenNumbers),"The view is shows only items satisfied from the specification");
           
        }

        [Test]
        public void VerifyTotals()
        {
            Assert.AreEqual(_targetCollection.Count,_target.Total);
            Assert.AreEqual(_target.Filtered,_evenNumbers.Count);
            Assert.AreEqual(_target.Shown,_oddNumbers.Count);
        }

        #region testhelpers

        private static bool IsEven(int number)
        {
            return (number % 2 == 0);
        }

        private class EvenNumberSpecification : Specification<int>
        {
            public override bool IsSatisfiedBy(int obj)
            {
                return IsEven(obj);
            }
        }
        #endregion



    }
}
