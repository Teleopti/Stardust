using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class ExplorerViewModelTest
    {
        private IExplorerViewModel _target;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IList<IRuleSetBag> _bagCollection;
        private IList<string> _accessibilities;
        private TypedBindingCollection<IActivity> _activities;
        private TypedBindingCollection<IShiftCategory> _categories;
        private IList<GridClassType> _classTypes;
        private IList<string> _operators;
        
        [SetUp]
        public void Setup()
        {
            _target = new ExplorerViewModel{DefaultSegment = 15};

            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _bagCollection = new List<IRuleSetBag>();

            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            _target.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());
            _target.AddRuleSet(WorkShiftRuleSetFactory.Create());
            
            _target.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));

            IRuleSetBag bag = new RuleSetBag();
            bag.Description = new Description("MyBag");
            bag.AddRuleSet(_ruleSetCollection[0]);

            _bagCollection.Add(bag);

            _target.SetRuleSetBagCollection(new ReadOnlyCollection<IRuleSetBag>(_bagCollection));
            _target.AddRuleSetBag(bag.NoneEntityClone());
            _target.SetFilteredRuleSetBagCollection(new ReadOnlyCollection<IRuleSetBag>(_bagCollection));


            _accessibilities = new List<string>();
            IList<KeyValuePair<DefaultAccessibility, string>> pair = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();
            foreach (KeyValuePair<DefaultAccessibility, string> value in pair)
                _accessibilities.Add(value.Value);
            _target.SetAccessibilityCollection(new ReadOnlyCollection<string>(_accessibilities));

            _activities = new TypedBindingCollection<IActivity>();
            _activities.Add(ActivityFactory.CreateActivity("Test1", Color.Black));
            _activities.Add(ActivityFactory.CreateActivity("Test2", Color.Blue));
            _activities.Add(ActivityFactory.CreateActivity("Test3", Color.Red));
            _target.SetActivityCollection(_activities);

            _categories = new TypedBindingCollection<IShiftCategory>();
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("Cat1"));
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("Cat2"));
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("Cat3"));
            _target.SetCategoryCollection(_categories);

            _classTypes = new List<GridClassType>();
            _classTypes.Add(new GridClassType(UserTexts.Resources.AbsoluteStart, typeof(ActivityAbsoluteStartExtender)));
            _classTypes.Add(new GridClassType(UserTexts.Resources.RelativeStart, typeof(ActivityRelativeStartExtender)));
            _classTypes.Add(new GridClassType(UserTexts.Resources.RelativeEnd, typeof(ActivityRelativeEndExtender)));
            _target.SetClassTypeCollection(_classTypes);

            _operators = new List<string>();
            IList<KeyValuePair<DefaultAccessibility, string>> pair1 = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();
            foreach (KeyValuePair<DefaultAccessibility, string> value in pair1)
                _operators.Add(value.Value);
            _target.SetOperatorLimitCollection(new ReadOnlyCollection<string>(_operators));

            _target.SetRightToLeft(true);
            _target.SetSelectedView(ShiftCreatorViewType.RuleSetBag);
            _target.SetShiftStartTime(TimeSpan.FromHours(8));
            _target.SetShiftEndTime(TimeSpan.FromHours(17));
            _target.SetWidthPerHour(17);
            _target.SetWidthPerPixel(1.45f);
            _target.SetVisualColumnWidth(970);
            _target.SetVisualizeGridColumnCount(19);
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.AreEqual(_ruleSetCollection.Count, _target.RuleSetCollection.Count);
            Assert.AreEqual(_ruleSetCollection, _target.FilteredRuleSetCollection);

            Assert.AreEqual(2, _target.RuleSetBagCollection.Count);
            Assert.AreEqual(_bagCollection, _target.FilteredRuleSetBagCollection);

            Assert.AreEqual(_accessibilities, _target.AccessibilityCollection);
            Assert.AreEqual(_activities, _target.ActivityCollection);
            Assert.AreEqual(_categories, _target.CategoryCollection);
            Assert.AreEqual(_classTypes, _target.ClassTypeCollection);
            Assert.AreEqual(_operators, _target.OperatorLimitCollection);

            Assert.AreEqual(true, _target.IsRightToLeft);
            Assert.AreEqual(ShiftCreatorViewType.RuleSetBag, _target.SelectedView);
            Assert.AreEqual(TimeSpan.FromHours(8), _target.ShiftStartTime);
            Assert.AreEqual(TimeSpan.FromHours(17), _target.ShiftEndTime);
            Assert.AreEqual(17, _target.WidthPerHour);
            Assert.AreEqual(1.45f, _target.WidthPerPixel);
            Assert.AreEqual(970, _target.VisualColumnWidth);
            Assert.AreEqual(19, _target.VisualizeGridColumnCount);
            Assert.AreEqual(new TimePeriod(8,0,9,0), _target.DefaultStartPeriod);
            Assert.AreEqual(new TimePeriod(17,0,18,0), _target.DefaultEndPeriod);
            Assert.AreEqual(TimeSpan.FromMinutes(15), _target.StartPeriodSegment);
            Assert.AreEqual(TimeSpan.FromMinutes(15), _target.EndPeriodSegment);

            _target.RemoveRuleSet(_ruleSetCollection[0]);
            Assert.AreEqual(1, _target.RuleSetCollection.Count);

            _target.RemoveBag(_bagCollection[0]);
            Assert.AreEqual(1, _target.RuleSetBagCollection.Count);
        }

		[Test]
		public void ShouldInitializeVisualizeGridColumnCount()
		{
			_target = new ExplorerViewModel();
			_target.VisualizeGridColumnCount.Should().Be.EqualTo(1);
		}
	}
}
