using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represent any person in the system
    /// </summary>
    public interface IPerson : IAggregateRoot, 
                                IBusinessUnitHierarchyEntity,
                                IChangeInfo
    {
        /// <summary>
        /// Gets the person's team at the given time.
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <returns></returns>
        ITeam MyTeam(DateOnly theDate);

        /// <summary>
        /// Terminal date
        /// </summary>
		DateOnly? TerminalDate { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        Name Name { get; set; }

        /// <summary>
        /// Gets the permission related information.
        /// </summary>
        /// <value>The permission information.</value>
        IPermissionInformation PermissionInformation { get; }

        /// <summary>
        /// Gets or sets the person period collecion.
        /// </summary>
        /// <value>The person period collecion.</value>
        IList<IPersonPeriod> PersonPeriodCollection { get; }

        /// <summary>
        /// Gets the person schedule period collection
        /// </summary>
        IList<ISchedulePeriod> PersonSchedulePeriodCollection { get; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        string Note { get; set; }

        /// <summary>
        /// Gets or sets the employment number.
        /// </summary>
        /// <value>The employment number.</value>
        string EmploymentNumber { get; set; }

        /// <summary>
        /// Gets a value indicating whether this person is an agent.
        /// </summary>
        bool IsAgent(DateOnly theDate);

        /// <summary>
        /// Gets a value indicating whether the person is a built in and shipped with the application.
        /// </summary>
        /// <value><c>true</c> if built in; otherwise, <c>false</c>.</value>
        bool BuiltIn { get; set; }

        /// <summary>
        /// Adds the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        void AddPersonPeriod(IPersonPeriod period);

        /// <summary>
        /// Deletes the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        void RemoveSchedulePeriod(ISchedulePeriod period);

        /// <summary>
        /// Deletes the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        void DeletePersonPeriod(IPersonPeriod period);

        /// <summary>
        /// Add schedule period to collection
        /// </summary>
        /// <param name="period"></param>
        void AddSchedulePeriod(ISchedulePeriod period);

        /// <summary>
        /// Periods the specified date only.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        IPersonPeriod Period(DateOnly dateOnly);

        /// <summary>
        /// Returns the schedule period for a specific date.
        /// If no period is found, null is returned.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        ISchedulePeriod SchedulePeriod(DateOnly dateOnly);

		/// <summary>
		/// Gets the schedule period start date.
		/// </summary>
		/// <param name="requestDate">The request date.</param>
		/// <returns></returns>
    	DateOnly? SchedulePeriodStartDate(DateOnly requestDate);

        /// <summary>
        /// Removes all person periods.
        /// </summary>
        void RemoveAllPersonPeriods();

        /// <summary>
        /// Removes all schedule periods.
        /// </summary>
        void RemoveAllSchedulePeriods();

        /// <summary>
        /// Gets the person periods within a period
        /// </summary>
        /// <param name="datePeriod"></param>
        /// <returns>A list of person periods or an empty list if no period found.</returns>
        IList<IPersonPeriod> PersonPeriods(DateOnlyPeriod datePeriod);

        /// <summary>
        /// Gets the person schedule periods within a period
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        IList<ISchedulePeriod> PersonSchedulePeriods(DateOnlyPeriod timePeriod);

		/// <summary>
		/// Gets the physical SchedulePeriods within a period
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
    	IList<ISchedulePeriod> PhysicalSchedulePeriods(DateOnlyPeriod period);

        /// <summary>
        /// Gets the next period. Returns null if provided period is the last period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        IPersonPeriod NextPeriod(IPersonPeriod period);

        /// <summary>
        /// Gets the previous period. Returns null if provided period is the first period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        IPersonPeriod PreviousPeriod(IPersonPeriod period);

        /// <summary>
        /// Gets the next schedule period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        ISchedulePeriod NextSchedulePeriod(ISchedulePeriod period);

        /// <summary>
        /// Gets the person rotation day restriction.
        /// </summary>
        /// <param name="personRestrictions">The person restrictions.</param>
        /// <param name="currentDate">The date.</param>
        /// <returns></returns>
        IList<IRotationRestriction> GetPersonRotationDayRestrictions(IEnumerable<IPersonRotation> personRestrictions, DateOnly currentDate);

        /// <summary>
        /// Gets the person availability day restriction.
        /// </summary>
        /// <param name="personRestrictions">The person restrictions.</param>
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
		IAvailabilityRestriction GetPersonAvailabilityDayRestriction(IEnumerable<IPersonAvailability> personRestrictions, DateOnly currentDate);
        
        /// <summary>
        /// Number of months employed
        /// </summary>
        int Seniority { get; }

        /// <summary>
        /// Gets the write protection.
        /// </summary>
        /// <value>The write protection.</value>
        IPersonWriteProtectionInfo PersonWriteProtection { get; }

        /// <summary>
        /// Gets or sets the workflow control set.
        /// </summary>
        /// <value>The workflow control set.</value>
        /// <remarks>
        IWorkflowControlSet WorkflowControlSet { get; set; }

        ///<summary>
        /// Used when checking rules when scheduling
        ///</summary>
        DayOfWeek FirstDayOfWeek { get; set; }

        /// <summary>
        /// Get the Virtual Schedule Period.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        IVirtualSchedulePeriod VirtualSchedulePeriod(DateOnly dateOnly);

		 /// <summary>
		 /// Get the virtual schedule period. If not exists, return the first one in the future (if exists)
		 /// </summary>
		 /// <param name="dateOnly"></param>
		 /// <returns></returns>
		  IVirtualSchedulePeriod VirtualSchedulePeriodOrNext(DateOnly dateOnly);

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <param name="loadPasswordPolicyService">The load password policy service.</param>
        /// <param name="userDetail">The user detail.</param>
        /// <returns></returns>
        bool ChangePassword(string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail);

		/// <summary>
		/// Changes the password.
		/// </summary>
		/// <param name="oldPassword">The old password.</param>
		/// <param name="newPassword">The new password.</param>
		/// <param name="loadPasswordPolicyService">The load password policy service.</param>
		/// <param name="userDetail">The user detail.</param>
		/// <returns></returns>
		IChangePasswordResultInfo ChangePassword(string oldPassword, string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail);

        /// <summary>
		/// Gets or sets the identity authentication info.
		/// </summary>
		/// <value>The identity authentication info.</value>
		IAuthenticationInfo AuthenticationInfo { get; set; }

        /// <summary>
        /// Gets or sets the application authentication info.
        /// </summary>
        /// <value>The application authentication info.</value>
        IApplicationAuthenticationInfo ApplicationAuthenticationInfo { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		ReadOnlyCollection<IOptionalColumnValue> OptionalColumnValueCollection { get; }

	    ISet<IAgentBadge> Badges { get; }

	    /// <summary>
    	/// 
    	/// </summary>
    	/// <param name="value"></param>
    	/// <param name="column"></param>
    	void AddOptionalColumnValue(IOptionalColumnValue value, IOptionalColumn column);

    	/// <summary>
    	/// 
    	/// </summary>
    	/// <param name="value"></param>
    	void RemoveOptionalColumnValue(IOptionalColumnValue value);

    	/// <summary>
    	/// 
    	/// </summary>
    	/// <param name="column"></param>
    	/// <returns></returns>
    	IOptionalColumnValue GetColumnValue(IOptionalColumn column);

        /// <summary>
        /// Worktime from contract or from schedule period
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns>work time</returns>
        TimeSpan AverageWorkTimeOfDay(DateOnly dateOnly);

		/// <summary>
		/// activates / reactivates person
		/// </summary>
		/// <param name="personAccountUpdater"></param>
	    void ActivatePerson(IPersonAccountUpdater personAccountUpdater);

		/// <summary>
		/// Terminates a person
		/// </summary>
		/// <param name="terminalDate"></param>
		/// <param name="personAccountUpdater"></param>
	    void TerminatePerson(DateOnly terminalDate, IPersonAccountUpdater personAccountUpdater);

		/// <summary>
		/// Change the start date for a person period.
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="period"></param>
	    void ChangePersonPeriodStartDate(DateOnly startDate, IPersonPeriod period);

	    void ChangeSchedulePeriodStartDate(DateOnly startDate, ISchedulePeriod schedulePeriod);
	    void ChangeTeam(ITeam team, IPersonPeriod personPeriod);
	    void AddSkill(IPersonSkill personSkill, IPersonPeriod personPeriod);
	    void ActivateSkill(ISkill skill, IPersonPeriod personPeriod);
	    void DeactivateSkill(ISkill skill, IPersonPeriod personPeriod);
	    void RemoveSkill(ISkill skill, IPersonPeriod personPeriod);
	    void ChangeSkillProficiency(ISkill skill, Percent proficiency, IPersonPeriod personPeriod);
	    void ResetPersonSkills(IPersonPeriod personPeriod);
		 void AddExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod);
		 void ResetExternalLogOn(IPersonPeriod personPeriod);
		 void RemoveExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod);
		 void AddBadge(IAgentBadge agentBadge, int silverToBronzeBadgeRate, int goldToSilverBadgeRate);
    }
}
