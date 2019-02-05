using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Scheduling result state holder
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-11-17    
    /// </remarks>
    public interface ISchedulingResultStateHolder
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
        ICollection<IPerson> LoadedAgents { get; set; }

        /// <summary>
        /// Gets or sets the skill days.
        /// </summary>
        /// <value>The skill days.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-11-10
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; set; }

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
		ISet<ISkill> Skills { get; set; }

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
		/// If the scheduler is opened in Team Leader Mode
		///</summary>
		bool TeamLeaderMode { get; set; }

		/// <summary>
		/// if the rule for maximum workday check should be used
		/// </summary>
		bool UseMaximumWorkday { get; set; }

		///<summary>
		/// If the modify uses all BusinessRules or just the mandatory
		///</summary>
		bool UseValidation { get; set; }

		/// <summary>
		/// Gets the rules to run.
		/// </summary>
		/// <returns></returns>
		INewBusinessRuleCollection GetRulesToRun();
		ISeniorityWorkDayRanks SeniorityWorkDayRanks { get; set; }
		IEnumerable<ExternalStaff> ExternalStaff { get; set; }
	    ResourceCalculationData ToResourceOptimizationData(bool considerShortBreaks, bool doIntraIntervalCalculation);
    }
}