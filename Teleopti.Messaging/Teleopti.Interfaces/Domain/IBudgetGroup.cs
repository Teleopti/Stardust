using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 9/28/2010
    /// </remarks>
    public interface IBudgetGroup : IAggregateRoot
    {
        /// <summary>
        /// Gets the skill collection.
        /// </summary>
        /// <value>The skill collection.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/28/2010
        /// </remarks>
        IEnumerable<ISkill> SkillCollection { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/28/2010
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the days per year.
        /// </summary>
        /// <value>The days per year.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/28/2010
        /// </remarks>
        int DaysPerYear { get; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/28/2010
        /// </remarks>
        ICccTimeZoneInfo TimeZone { get; set; }

        ///<summary>
        /// The custom shrinkages defined for this budget group
        ///</summary>
        IEnumerable<ICustomShrinkage> CustomShrinkages { get; }

        ///<summary>
        /// The custom effiency shrinkages defined for this budget group
        ///</summary>
        IEnumerable<ICustomEfficiencyShrinkage> CustomEfficiencyShrinkages { get; }

        /// <summary>
        /// Adds the skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/28/2010
        /// </remarks>
        void AddSkill(ISkill skill);

        /// <summary>
        /// Removes all skills.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 9/29/2010
        /// </remarks>
        void RemoveAllSkills();

        ///<summary>
        /// Add a custom shrinkage
        ///</summary>
        ///<param name="customShrinkage"></param>
        void AddCustomShrinkage(ICustomShrinkage customShrinkage);

        ///<summary>
        /// Removes a custom shrinkage
        ///</summary>
        ///<param name="customShrinkage"></param>
        void RemoveCustomShrinkage(ICustomShrinkage customShrinkage);

        ///<summary>
        /// Add a custom efficiency shrinkage
        ///</summary>
        ///<param name="customEfficiencyShrinkage"></param>
        void AddCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage);

        ///<summary>
        /// Removes a custom efficiency shrinkage
        ///</summary>
        ///<param name="customEfficiencyShrinkage"></param>
        void RemoveCustomEfficiencyShrinkage(ICustomEfficiencyShrinkage customEfficiencyShrinkage);

        ///<summary>
        /// Check if the suppled id for a custom shrinkage is valid for this budget group.
        ///</summary>
        ///<param name="customShrinkageId">The id of the custom shrinkage definition.</param>
        ///<returns>true if the shrinkage is defined, otherwise false.</returns>
        bool IsCustomShrinkage(Guid customShrinkageId);

        ///<summary>
        /// Check if the suppled id for a custom efficiency shrinkage is valid for this budget group.
        ///</summary>
        ///<param name="customEfficiencyShrinkageId">The id of the custom efficiency shrinkage definition.</param>
        ///<returns>true if the efficiency shrinkage is defined, otherwise false.</returns>
        bool IsCustomEfficiencyShrinkage(Guid customEfficiencyShrinkageId);

        /// <summary>
        /// Tries the set days per year.
        /// </summary>
        /// <param name="daysPerYear">The days per year.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/14/2010
        /// </remarks>
        void TrySetDaysPerYear(int daysPerYear);

        /// <summary>
        /// Update an existed custom shrinkage with new value
        /// </summary>
        /// <param name="id">custom shrinkage Id</param>
        /// <param name="newCustomShrinkage">new custom shrinkage</param>
        void UpdateCustomShrinkage(Guid id, ICustomShrinkage newCustomShrinkage);

        /// <summary>
        /// Update an existed custom efficiency shrinkage with new value
        /// </summary>
        /// <param name="id">custom efficiency shrinkage Id</param>
        /// <param name="newCustomEfficiencyShrinkage">new custom efficiency shrinkage</param>
        void UpdateCustomEfficiencyShrinkage(Guid id, ICustomEfficiencyShrinkage newCustomEfficiencyShrinkage);

        /// <summary>
        /// Get shrinkage by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Custom shrinkage</returns>
        ICustomShrinkage GetShrinkage(Guid id);

        /// <summary>
        /// Get efficiency shrinkage by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Custom efficiency shrinkage</returns>
        ICustomEfficiencyShrinkage GetEfficiencyShrinkage(Guid id);
    }
}
