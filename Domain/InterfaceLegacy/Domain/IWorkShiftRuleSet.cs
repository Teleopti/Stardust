using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IWorkShiftRuleSet : IAggregateRoot, 
                                            ICloneableEntity<IWorkShiftRuleSet>
    {
        Description Description { get; set; }

		bool OnlyForRestrictions { get; set; }

        ReadOnlyCollection<IWorkShiftExtender> ExtenderCollection { get; }

        ReadOnlyCollection<IWorkShiftLimiter> LimiterCollection { get; }

        ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection { get; }

        void RemoveRuleSetBag(IRuleSetBag ruleSetBag);

        IWorkShiftTemplateGenerator TemplateGenerator { get; set; }

        DefaultAccessibility DefaultAccessibility { get; set; }

        IEnumerable<DayOfWeek> AccessibilityDaysOfWeek { get; }

        IEnumerable<DateOnly> AccessibilityDates { get; }

		void AddAccessibilityDayOfWeek(DayOfWeek dayOfWeek);

        void RemoveAccessibilityDayOfWeek(DayOfWeek dayOfWeek);

        void AddExtender(IWorkShiftExtender extender);

        void AddLimiter(IWorkShiftLimiter limiter);

	    void InsertExtender(int index, IWorkShiftExtender extender);

        void DeleteExtender(IWorkShiftExtender extender);

        void DeleteLimiter(IWorkShiftLimiter limiter);

        void AddAccessibilityDate(DateOnly date);

        void RemoveAccessibilityDate(DateOnly date);

        bool IsValidDate(DateOnly dateToCheck);

        void SwapExtenders(IWorkShiftExtender left, IWorkShiftExtender right);
    }

}