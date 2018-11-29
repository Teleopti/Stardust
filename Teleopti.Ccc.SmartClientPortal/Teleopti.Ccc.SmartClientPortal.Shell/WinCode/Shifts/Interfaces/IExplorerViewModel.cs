using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IExplorerViewModel
    {
        int DefaultSegment { get; set; }

        TimePeriod DefaultStartPeriod { get; }

        TimePeriod DefaultEndPeriod { get; }

        TimeSpan StartPeriodSegment { get; }

        TimeSpan EndPeriodSegment { get; }

        ShiftCreatorViewType SelectedView { get; }

        TimeSpan ShiftStartTime { get; }

        TimeSpan ShiftEndTime { get; }

        int VisualizeGridColumnCount { get; }

        float WidthPerHour { get; }

        float WidthPerPixel { get; }

        float VisualColumnWidth { get; }

        ReadOnlyCollection<IWorkShiftRuleSet> RuleSetCollection { get; }

        ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection { get; }

        ReadOnlyCollection<IWorkShiftRuleSet> FilteredRuleSetCollection { get; }

        ReadOnlyCollection<IRuleSetBag> FilteredRuleSetBagCollection { get; }

        TypedBindingCollection<IActivity> ActivityCollection { get; }

        TypedBindingCollection<IShiftCategory> CategoryCollection { get; }

        ReadOnlyCollection<string> OperatorLimitCollection { get; }

        ReadOnlyCollection<string> AccessibilityCollection { get; }

        IList<GridClassType> ClassTypeCollection { get; }

        bool IsRightToLeft { get; }

        void SetActivityCollection(TypedBindingCollection<IActivity> activities);

        void SetCategoryCollection(TypedBindingCollection<IShiftCategory> categories);

        void SetOperatorLimitCollection(ReadOnlyCollection<string> operatorLimits);

        void SetAccessibilityCollection(ReadOnlyCollection<string> accessibilities);

        void SetRuleSetCollection(ReadOnlyCollection<IWorkShiftRuleSet> ruleSets);

        void SetRuleSetBagCollection(ReadOnlyCollection<IRuleSetBag> bags);

        void SetFilteredRuleSetCollection(ReadOnlyCollection<IWorkShiftRuleSet> filteredRuleSetCollection);

        void SetFilteredRuleSetBagCollection(ReadOnlyCollection<IRuleSetBag> filteredRuleSetBagCollection);

        void SetSelectedView(ShiftCreatorViewType view);

        void SetShiftStartTime(TimeSpan startTime);

        void SetShiftEndTime(TimeSpan endTime);

        void SetVisualizeGridColumnCount(int count);

        void SetClassTypeCollection(IList<GridClassType> classTypes);

        void SetRightToLeft(bool rightToLeft);

        void SetWidthPerHour(float width);

        void SetWidthPerPixel(float width);

        void SetVisualColumnWidth(float width);

        void AddRuleSet(IWorkShiftRuleSet ruleSet);

        void RemoveRuleSet(IWorkShiftRuleSet ruleSet);

        void AddRuleSetBag(IRuleSetBag bag);

        void RemoveBag(IRuleSetBag bag);
    }
}
