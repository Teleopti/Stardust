using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Represent any person in the system
	/// </summary>
	public interface IPerson : IAggregateRoot,
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
		Name Name { get; }

		void SetName(Name value);

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
		/// Sets the employment number.
		/// </summary>
		/// <param name="value">The employment number.</param>
		void SetEmploymentNumber(string value);

		/// <summary>
		/// Gets the employment number.
		/// </summary>
		/// <value>The employment number.</value>
		string EmploymentNumber { get; }

		/// <summary>
		/// Gets a value indicating whether this person is an agent.
		/// </summary>
		bool IsAgent(DateOnly theDate);

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
		/// <param name="date">The date only.</param>
		/// <returns></returns>
		IPersonPeriod Period(DateOnly date);

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
		/// Gets the person rotation day restriction.
		/// </summary>
		/// <param name="personRestrictions">The person restrictions.</param>
		/// <param name="currentDate">The date.</param>
		/// <returns></returns>
		IList<IRotationRestriction> GetPersonRotationDayRestrictions(IEnumerable<IPersonRotation> personRestrictions, DateOnly currentDate);

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
		/// 
		/// </summary>
		/// <returns></returns>
		ReadOnlyCollection<IOptionalColumnValue> OptionalColumnValueCollection { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="column"></param>
		void SetOptionalColumnValue(IOptionalColumnValue value, IOptionalColumn column);

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
		PersonWorkDay AverageWorkTimeOfDay(DateOnly dateOnly);

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
		/// <param name="personLeavingUpdater"></param>
		void TerminatePerson(DateOnly terminalDate, IPersonAccountUpdater personAccountUpdater, IPersonLeavingUpdater personLeavingUpdater = null);

		/// <summary>
		/// Change the start date for a person period.
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="period"></param>
		void ChangePersonPeriodStartDate(DateOnly startDate, IPersonPeriod period);

		void ChangeSchedulePeriodStartDate(DateOnly startDate, ISchedulePeriod schedulePeriod);
		void ChangeTeam(ITeam team, IPersonPeriod personPeriod);
		void AddSkill(IPersonSkill personSkill, IPersonPeriod personPeriod);
		void AddSkill(ISkill skill, DateOnly personPeriodDate);
		void ActivateSkill(ISkill skill, IPersonPeriod personPeriod);
		void DeactivateSkill(ISkill skill, IPersonPeriod personPeriod);
		void RemoveSkill(ISkill skill, IPersonPeriod personPeriod);
		void ChangeSkillProficiency(ISkill skill, Percent proficiency, IPersonPeriod personPeriod);
		void ResetPersonSkills(IPersonPeriod personPeriod);
		void AddExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod);
		void ResetExternalLogOn(IPersonPeriod personPeriod);
		void RemoveExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod);
		bool IsTerminated(DateOnly? date = null);
		PersonWorkDay[] AverageWorkTimes(DateOnlyPeriod period);
		ISiteOpenHour SiteOpenHour(DateOnly dateOnly);
		void RemoveAllPeriodsAfter(DateOnly date);
		bool IsExternalAgent { get; }
		void AddPersonEmploymentChangeEvent(PersonEmploymentChangedEvent personEmploymentChangedEvent);
	}
}
