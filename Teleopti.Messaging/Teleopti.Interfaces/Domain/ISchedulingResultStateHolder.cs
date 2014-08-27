using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Scheduling result state holder
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-11-17    
    /// </remarks>
    public interface ISchedulingResultStateHolder : IDisposable
    {
        /// <summary>
        /// Gets or sets all person accounts.
        /// </summary>
        /// <value>All person accounts.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IDictionary<IPerson, IPersonAccountCollection> AllPersonAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [skip resource calculation].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [skip resource calculation]; otherwise, <c>false</c>.
        /// </value>
        bool SkipResourceCalculation { get; set; }

        /// <summary>
        /// Gets or sets the persons in organization.
        /// </summary>
        /// <value>The persons in organization.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-10
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        ICollection<IPerson> PersonsInOrganization { get; set; }

        /// <summary>
        /// Gets or sets the skill days.
        /// </summary>
        /// <value>The skill days.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-10
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IDictionary<ISkill, IList<ISkillDay>> SkillDays { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>The schedules.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-10
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IScheduleDictionary Schedules { get; set; }

        /// <summary>
        /// Gets the skills.
        /// </summary>
        /// <value>The skills.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-10
        /// </remarks>
        IList<ISkill> Skills { get; }

        /// <summary>
        /// Gets the visible skills.
        /// </summary>
        /// <value>The visible skills.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        IList<ISkill> VisibleSkills { get; }

       /// <summary>
        /// Return SkillDays on the DateOnlys.
        /// </summary>
        /// <param name="theDateList">The DateOnlys.</param>
        /// <returns></returns>
        IList<ISkillDay> SkillDaysOnDateOnly(IList<DateOnly> theDateList);

		
        /// <summary>
        /// Gets the skill staff period holder.
        /// </summary>
        /// <value>The skill staff period holder.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-11-17    
        /// /// </remarks>
        ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }

        ///<summary>
        ///</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IEnumerable<IShiftCategory> ShiftCategories { get; set; }

		///<summary>
		/// If the scheduler is opened in Team Leader Mode
		///</summary>
		bool TeamLeaderMode { get; set; }

		///<summary>
		/// If the modify uses all BusinessRules or just the mandatory
		///</summary>
		bool UseValidation { get; set; }

		/// <summary>
		/// Gets the rules to run.
		/// </summary>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		INewBusinessRuleCollection GetRulesToRun();

		/// <summary>
		/// 
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		IList<IOptionalColumn> OptionalColumns { get; set; }
		
		/// <summary>
		/// Gets the visible skills without max seat and non-blend skills
		/// </summary>
		/// <value>The visible skills.</value>
	    IList<ISkill> NonVirtualSkills { get; }

        /// <summary>
        /// If rule for min week work time should be used
        /// </summary>
        bool UseMinWeekWorkTime { get; set; }
    }
}