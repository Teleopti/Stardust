using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A set of rules used when creating work shifts
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-20
    /// </remarks>
    public interface IWorkShiftRuleSet : IAggregateRoot, 
                                            ICloneableEntity<IWorkShiftRuleSet>
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-25
        /// </remarks>
        Description Description { get; set; }

		/// <summary>
		/// Gets or sets flag for use only with restrictions
		/// </summary>
		bool OnlyForRestrictions { get; set; }

        /// <summary>
        /// Gets the extender collection.
        /// </summary>
        /// <value>The extender collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        ReadOnlyCollection<IWorkShiftExtender> ExtenderCollection { get; }

        /// <summary>
        /// Gets the limiter collection.
        /// </summary>
        /// <value>The limiter collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        ReadOnlyCollection<IWorkShiftLimiter> LimiterCollection { get; }

        /// <summary>
        /// Gets the rule set bag collection.
        /// </summary>
        /// <value>The rule set bag collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-31
        /// </remarks>
        ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection { get; }

        /// <summary>
        /// Removes the rule set bag specified in parameter.
        /// </summary>
        /// <param name="ruleSetBag">The rule set bag.</param>
        void RemoveRuleSetBag(IRuleSetBag ruleSetBag);

        /// <summary>
        /// Gets the template generator.
        /// </summary>
        /// <value>The template generator.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        IWorkShiftTemplateGenerator TemplateGenerator { get; set; }

        /// <summary>
        /// Gets the default accessibility.
        /// </summary>
        /// <value>The default accessibility.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        DefaultAccessibility DefaultAccessibility { get; set; }

        /// <summary>
        /// Gets the accessibility days of week.
        /// </summary>
        /// <value>The accessibility days of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        ReadOnlyCollection<DayOfWeek> AccessibilityDaysOfWeek { get; }

        /// <summary>
        /// Gets the accessibility dates.
        /// </summary>
        /// <value>The accessibility dates.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        ReadOnlyCollection<DateTime> AccessibilityDates { get; }


        /// <summary>
        /// Adds the accessibility day of week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        void AddAccessibilityDayOfWeek(DayOfWeek dayOfWeek);

        /// <summary>
        /// Removes the accessibility day of week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        void RemoveAccessibilityDayOfWeek(DayOfWeek dayOfWeek);

        /// <summary>
        /// Adds an extender.
        /// </summary>
        /// <param name="extender">The extender.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        void AddExtender(IWorkShiftExtender extender);

        /// <summary>
        /// Adds a limiter.
        /// </summary>
        /// <param name="limiter">The limiter.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        void AddLimiter(IWorkShiftLimiter limiter);

        /// <summary>
        /// Deletes the extender.
        /// </summary>
        /// <param name="extender">The extender.</param>
        void DeleteExtender(IWorkShiftExtender extender);

        /// <summary>
        /// Deletes the limiter.
        /// </summary>
        /// <param name="limiter">The limiter.</param>
        void DeleteLimiter(IWorkShiftLimiter limiter);

        /// <summary>
        /// Adds the accessibility date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        void AddAccessibilityDate(DateTime dateTime);

        /// <summary>
        /// Removes the accessibility date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        void RemoveAccessibilityDate(DateTime dateTime);

        /// <summary>
        /// Determines whether [is valid date] [the specified date].
        /// </summary>
        /// <param name="dateToCheck">The date to check.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid date] [the specified date]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        bool IsValidDate(DateOnly dateToCheck);

        /// <summary>
        /// Swaps the extenders.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        void SwapExtenders(IWorkShiftExtender left, IWorkShiftExtender right);

        #region RK - Lägg dessa utanför denna entitiet!

        /// <summary>
        /// Mins the max work time.
        /// </summary>
        /// <param name="ruleSetProjectionService">The rule set projection service.</param>
        /// <param name="effectiveRestriction">The effective restriction.</param>
        /// <returns></returns>
        IWorkTimeMinMax MinMaxWorkTime(IRuleSetProjectionService ruleSetProjectionService, IEffectiveRestriction effectiveRestriction);

       
        #endregion
    }

}