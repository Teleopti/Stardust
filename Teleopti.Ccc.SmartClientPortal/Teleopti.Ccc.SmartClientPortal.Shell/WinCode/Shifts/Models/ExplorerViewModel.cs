using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models
{
    public class ExplorerViewModel : IExplorerViewModel
    {
        private IList<IWorkShiftRuleSet> _ruleSets;
        private IList<IRuleSetBag> _ruleSetbags;

		public ExplorerViewModel()
		{
			VisualizeGridColumnCount = 1;
		}

        public int DefaultSegment{ get; set;}

        public TimePeriod DefaultStartPeriod
        {
            get
            {
                return new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
            }
        }

        public TimePeriod DefaultEndPeriod
        {
            get
            {
                return new TimePeriod(TimeSpan.FromHours(17), TimeSpan.FromHours(18));
            }
        }

        public TimeSpan StartPeriodSegment
        {
            get
            {
                return TimeSpan.FromMinutes(DefaultSegment);
            }
        }

        public TimeSpan EndPeriodSegment
        {
            get
            {
                return StartPeriodSegment;
            }
        }

        public ShiftCreatorViewType SelectedView
        {
            get;
            private set;
        }

        public TimeSpan ShiftStartTime
        {
            get;
            private set;
        }

        public TimeSpan ShiftEndTime
        {
            get;
            private set;
        }

        public int VisualizeGridColumnCount
        {
            get;
            private set;
        }

        public float WidthPerHour
        {
            get;
            private set;
        }

        public float WidthPerPixel
        {
            get;
            private set;
        }

        public ReadOnlyCollection<IWorkShiftRuleSet> RuleSetCollection
        {
            get
            {
                return new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSets);
            }
            set
            {
                _ruleSets = value.ToList();
            }
        }

        public ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection
        {
            get
            {
                return new ReadOnlyCollection<IRuleSetBag>(_ruleSetbags);
            }
            set
            {
                _ruleSetbags = value.ToList();
            }
        }

        public ReadOnlyCollection<IWorkShiftRuleSet> FilteredRuleSetCollection
        {
            get;
            private set;
        }

        public ReadOnlyCollection<IRuleSetBag> FilteredRuleSetBagCollection
        {
            get;
            private set;
        }

        public TypedBindingCollection<IActivity> ActivityCollection
        {
            get;
            private set;
        }

        public TypedBindingCollection<IShiftCategory> CategoryCollection
        {
            get;
            private set;
        }

        public ReadOnlyCollection<string> OperatorLimitCollection
        {
            get;
            private set;
        }

        public ReadOnlyCollection<string> AccessibilityCollection
        {
            get;
            private set;
        }

        public IList<GridClassType> ClassTypeCollection
        {
            get;
            private set;
        }

        public bool IsRightToLeft
        {
            get;
            private set;
        }

        public float VisualColumnWidth
        {
            get;
            private set;
        }

        public void SetActivityCollection(TypedBindingCollection<IActivity> activities)
        {
            ActivityCollection = activities;
        }

        public void SetCategoryCollection(TypedBindingCollection<IShiftCategory> categories)
        {
            CategoryCollection = categories;
        }

        public void SetOperatorLimitCollection(ReadOnlyCollection<string> operatorLimits)
        {
            OperatorLimitCollection = operatorLimits;
        }

        public void SetAccessibilityCollection(ReadOnlyCollection<string> accessibilities)
        {
            AccessibilityCollection = accessibilities;
        }

        public void SetRuleSetCollection(ReadOnlyCollection<IWorkShiftRuleSet> ruleSets)
        {
            RuleSetCollection = ruleSets;
        }

        public void SetRuleSetBagCollection(ReadOnlyCollection<IRuleSetBag> bags)
        {
            RuleSetBagCollection = bags;
        }

        public void SetFilteredRuleSetCollection(ReadOnlyCollection<IWorkShiftRuleSet> filteredRuleSetCollection)
        {
            FilteredRuleSetCollection = filteredRuleSetCollection;
        }

        public void AddRuleSet(IWorkShiftRuleSet ruleSet)
        {
            _ruleSets.Add(ruleSet);
        }

        public void SetFilteredRuleSetBagCollection(ReadOnlyCollection<IRuleSetBag> filteredRuleSetBagCollection)
        {
            FilteredRuleSetBagCollection = filteredRuleSetBagCollection;
        }

        public void AddRuleSetBag(IRuleSetBag bag)
        {
            _ruleSetbags.Add(bag);
        }

        public void RemoveRuleSet(IWorkShiftRuleSet ruleSet)
        {
            _ruleSets.Remove(ruleSet);
        }

        public void RemoveBag(IRuleSetBag bag)
        {
            _ruleSetbags.Remove(bag);
        }

        public void SetSelectedView(ShiftCreatorViewType view)
        {
            SelectedView = view;
        }

        public void SetShiftStartTime(TimeSpan startTime)
        {
            ShiftStartTime = startTime;
        }

        public void SetShiftEndTime(TimeSpan endTime)
        {
            ShiftEndTime = endTime;
        }

        public void SetVisualizeGridColumnCount(int count)
        {
            VisualizeGridColumnCount = count;
        }

        public void SetClassTypeCollection(IList<GridClassType> classTypes)
        {
            ClassTypeCollection = classTypes;
        }

        public void SetRightToLeft(bool rightToLeft)
        {
            IsRightToLeft = rightToLeft;
        }

        public void SetWidthPerHour(float width)
        {
            WidthPerHour = width;
        }

        public void SetWidthPerPixel(float width)
        {
            WidthPerPixel = width;
        }

        public void SetVisualColumnWidth(float width)
        {
            VisualColumnWidth = width;
        }

    }
}
