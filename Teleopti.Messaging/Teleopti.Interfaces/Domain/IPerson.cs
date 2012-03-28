using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Represent any person in the system
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-28
    /// </remarks>
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
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-04
        /// </remarks>
        DateOnly? TerminalDate { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        Name Name { get; set; }

        /// <summary>
        /// Gets the part of unique.
        /// </summary>
        /// <value>The part of unique.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        Guid? PartOfUnique { get; }

        /// <summary>
        /// Gets the permission related information.
        /// </summary>
        /// <value>The permission information.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        IPermissionInformation PermissionInformation { get; }

        /// <summary>
        /// Gets or sets the person period collecion.
        /// </summary>
        /// <value>The person period collecion.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        IList<IPersonPeriod> PersonPeriodCollection { get; }

        /// <summary>
        /// Gets the person schedule period collection
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-07
        /// </remarks>
        IList<ISchedulePeriod> PersonSchedulePeriodCollection { get; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-29
        /// </remarks>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-22
        /// </remarks>
        string Note { get; set; }

        /// <summary>
        /// Gets or sets the employment number.
        /// </summary>
        /// <value>The employment number.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-02-04
        /// </remarks>
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
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        void AddPersonPeriod(IPersonPeriod period);

        /// <summary>
        /// Determines whether [is ok to add person period] [the specified date only].
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns>
        /// 	<c>true</c> if [is ok to add person period] [the specified date only]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-11-03
        /// </remarks>
        bool IsOkToAddPersonPeriod(DateOnly dateOnly);

        /// <summary>
        /// Determines whether [is ok to add schedule period] [the specified date only].
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns>
        /// 	<c>true</c> if [is ok to add schedule period] [the specified date only]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2011-06-08
        /// </remarks>
        bool IsOkToAddSchedulePeriod(DateOnly dateOnly);

        /// <summary>
        /// Deletes the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-19
        /// </remarks>
        void RemoveSchedulePeriod(ISchedulePeriod period);

        /// <summary>
        /// Deletes the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-19
        /// </remarks>
        void DeletePersonPeriod(IPersonPeriod period);

        /// <summary>
        /// Add schedule period to collection
        /// </summary>
        /// <param name="period"></param>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-07
        /// </remarks>
        void AddSchedulePeriod(ISchedulePeriod period);

        /// <summary>
        /// Periods the specified date only.
        /// </summary>
        /// <param name="dateOnly">The date only.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-03-18    
        /// /// </remarks>
        IPersonPeriod Period(DateOnly dateOnly);

        /// <summary>
        /// Returns the schedule period for a specific date.
        /// If no period is found, null is returned.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-07
        /// </remarks>
        ISchedulePeriod SchedulePeriod(DateOnly dateOnly);

        /// <summary>
        /// Removes all person periods.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-02
        /// </remarks>
        void RemoveAllPersonPeriods();

        /// <summary>
        /// Removes all schedule periods.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-03
        /// </remarks>
        void RemoveAllSchedulePeriods();

        /// <summary>
        /// Gets the person periods within a period
        /// </summary>
        /// <param name="datePeriod"></param>
        /// <returns>A list of person periods or an empty list if no period found.</returns>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-04
        /// </remarks>
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
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-11    
        /// /// </remarks>
        IList<IRotationRestriction> GetPersonRotationDayRestrictions(IEnumerable<IPersonRotation> personRestrictions, DateOnly currentDate);

        /// <summary>
        /// Gets the person availability day restriction.
        /// </summary>
        /// <param name="personRestrictions">The person restrictions.</param>
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-20    
        /// /// </remarks>
		IAvailabilityRestriction GetPersonAvailabilityDayRestriction(IEnumerable<IPersonAvailability> personRestrictions, DateOnly currentDate);
        
        /// <summary>
        /// Number of months employed
        /// </summary>
        int Seniority { get; }

        /// <summary>
        /// Gets the write protection.
        /// </summary>
        /// <value>The write protection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-03
        /// </remarks>
        IPersonWriteProtectionInfo PersonWriteProtection { get; }

        /// <summary>
        /// Gets or sets the workflow control set.
        /// </summary>
        /// <value>The workflow control set.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-04-15
        /// </remarks>
        IWorkflowControlSet WorkflowControlSet { get; set; }

        ///<summary>
        /// Used when checking rules when scheduling
        ///</summary>
        DayOfWeek FirstDayOfWeek { get; set; }

        /// <summary>
        /// Determines whether this date is a work day according to the contract schedule
        /// </summary>
        /// <param name="dateOnlyDate">The requested date.</param>
        /// <returns>
        /// 	<c>true</c> if [is date contract schedule work day] [the specified requested date]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-09-29
        /// </remarks>
        bool IsDateContractScheduleWorkday(DateOnly dateOnlyDate);

        /// <summary>
        /// The work time based on contract schedule.
        /// </summary>
        /// <param name="dateOnlyDate">The requested date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-10-20
        /// </remarks>
        TimeSpan ContractScheduleWorkTime(DateOnly dateOnlyDate);

        /// <summary>
        /// Returns the ContractTime for a period
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// calaculates by looking at averageday contracttime
        /// Created by: henrika
        /// Created date: 2008-10-21
        /// </remarks>
        TimeSpan ContractScheduleWorkTime(DateOnlyPeriod period);

        /// <summary>
        /// Returns th number of Days Off for a period
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-10-21
        /// </remarks>
        int ContractScheduleDaysOff(DateOnlyPeriod period);

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
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-19
        /// </remarks>
        bool ChangePassword(string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail);

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="loadPasswordPolicyService">The load password policy service.</param>
        /// <param name="userDetail">The user detail.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-08-23
        /// </remarks>
        bool ChangePassword(string oldPassword, string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail);

        /// <summary>
        /// Gets or sets the windows authentication info.
        /// </summary>
        /// <value>The windows authentication info.</value>
        IWindowsAuthenticationInfo WindowsAuthenticationInfo { get; set; }
    }
}
