using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Event args for changed resources
	/// </summary>
	public class ResourceChangedEventArgs : EventArgs
	{
		private readonly IEnumerable<DateOnly> _changedDays;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceChangedEventArgs"/> class.
		/// </summary>
		/// <param name="changedDays">The changed days.</param>
		public ResourceChangedEventArgs(IEnumerable<DateOnly> changedDays)
		{
			_changedDays = changedDays;
		}

		/// <summary>
		/// Gets the changed days.
		/// </summary>
		/// <value>The changed days.</value>
		public IEnumerable<DateOnly> ChangedDays
		{
			get { return _changedDays; }
		}
	}

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
        /// Raises the ResourcesChanged event.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-04
        /// </remarks>
        void OnResourcesChanged(IList<DateOnly> changedDays);

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
        IList<IShiftCategory> ShiftCategories { get; set; }

        /// <summary>
		/// Occurs when [resources changed].
		/// </summary>
		event EventHandler<ResourceChangedEventArgs> ResourcesChanged;

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
    }
}