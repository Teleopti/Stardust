using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Contains scheduling related operations.
    /// </summary>
    [ServiceContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
        Name = "TeleoptiSchedulingService",
        ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiSchedulingService")]
    public interface ITeleoptiSchedulingService
    {
        /// <summary>
        /// Gets the availble activities.
        /// </summary>
        /// <param name="loadOptionDto">The load options.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ActivityDto> GetActivities(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets the available absences.
        /// </summary>
        /// <param name="loadOptionDto">The load option.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<AbsenceDto> GetAbsences(AbsenceLoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets schedule information for one day.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
		[OperationContract]
		[Obsolete("This method is obsolete, use GetSchedulesByQuery with appropriate query dto instead, to reduce data sent over the network.")]
        SchedulePartDto GetSchedulePart(PersonDto person, DateOnlyDto startDate, string timeZoneId);

        /// <summary>
        /// Gets schedule information for days between the given dates.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
		[OperationContract]
		[Obsolete("This method is obsolete, use GetSchedulesByQuery with appropriate query dto instead, to reduce data sent over the network.")]
        ICollection<SchedulePartDto> GetScheduleParts(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        /// <summary>
        /// Gets schedule information for all persons between the given dates.
        /// </summary>
        /// <param name="personList">The list of people to load schedule for.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
        [Obsolete("This method is obsolete, use GetSchedulesByQuery instead.")]
        [OperationContract]
        ICollection<SchedulePartDto> GetSchedulePartsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        /// <summary>
        /// Gets Payroll time export information for all persons between the given dates.
        /// </summary>
        /// <param name="personList">The list of people to load schedule for.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
        [OperationContract]
		ICollection<PayrollBaseExportDto> GetTeleoptiTimeExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        /// <summary>
        /// Gets Payroll detailed export information for all persons between the given dates.
        /// </summary>
        /// <param name="personList">The list of people to load schedule for.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
        [OperationContract]
		ICollection<PayrollBaseExportDto> GetTeleoptiDetailedExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

	    /// <summary>
	    /// Gets Payroll activities export information for all persons between the given dates.
	    /// </summary>
	    /// <param name="personList">The list of people to load schedule for.</param>
	    /// <param name="startDate">The start date.</param>
	    /// <param name="endDate">The end date.</param>
	    /// <param name="timeZoneId">The time zone.</param>
	    /// <returns></returns>
	    /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
	    [OperationContract]
		ICollection<PayrollBaseExportDto> GetTeleoptiActivitiesExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        /// <summary>
        /// Gets schedule information for days between the given dates using the specified load options.
        /// </summary>
        /// <param name="scheduleLoadOptionDto">The schedule load options.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <returns></returns>
        /// <remarks>All local time and date information on periods inside the SchedulePartDto will reflect the supplied time zone.</remarks>
        [OperationContract, FaultContract(typeof(FaultException))]
		[Obsolete("This method is obsolete, use GetSchedulesByQuery with appropriate query dto instead, to reduce data sent over the network.")]
		ICollection<SchedulePartDto> GetSchedules(ScheduleLoadOptionDto scheduleLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        /// <summary>
        /// Saves updated schedule information.
        /// </summary>
        /// <param name="schedulePartDto">The updated schedule information.</param>
        /// <remarks>Restrictions or meetings will not be saved using this operation.</remarks>
        [OperationContract]
				[Obsolete("This method is obsolete, instead use any of the available task based methods to modify a schedule.")]
        void SaveSchedulePart(SchedulePartDto schedulePartDto);

        /// <summary>
        /// Get shift categories belonging to the given persons rule set bag between the given dates.
        /// </summary>
        /// <param name="personDto">The person.</param>
        /// <param name="startDateOnlyDto">The start date.</param>
        /// <param name="endDateOnlyDto">The end date.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ShiftCategoryDto> GetShiftCategoriesBelongingToRuleSetBag(PersonDto personDto, DateOnlyDto startDateOnlyDto, DateOnlyDto endDateOnlyDto);

        /// <summary>
        /// Deletes the given person request.
        /// </summary>
        /// <param name="personRequestDto">The person request.</param>
        [OperationContract]
        void DeletePersonRequest(PersonRequestDto personRequestDto);

        /// <summary>
        /// Saves the person request.
        /// </summary>
        /// <param name="personRequestDto">The person request.</param>
        /// <returns></returns>
        /// <remarks>Absence requests cannot be saved using this operation.</remarks>
        [OperationContract, FaultContract(typeof(FaultException))]
        PersonRequestDto SavePersonRequest(PersonRequestDto personRequestDto);

        /// <summary>
        /// Gets all requests for the given person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonRequestDto> GetAllPersonRequests(PersonDto person);

        /// <summary>
        /// Gets the person requests between the start and end date involving the given person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="utcStartDate">The start date.</param>
        /// <param name="utcEndDate">The end date.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonRequestDto> GetPersonRequests(PersonDto person, DateTime utcStartDate, DateTime utcEndDate);

        /// <summary>
        /// Gets all pending requests and requests modified within the given period.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="utcStartDate">The start date.</param>
        /// <param name="utcEndDate">The end date.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonRequestDto> GetAllRequestModifiedWithinPeriodOrPending(PersonDto person, DateTime utcStartDate, DateTime utcEndDate);

        /// <summary>
        /// Creates a new shift trade request without saving to the database.
        /// </summary>
        /// <param name="requester">The person initiating the shift trade.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="shiftTradeSwapDetailDtos">The details for the shift trade.</param>
        /// <returns></returns>
        /// <remarks>This operation must be used to get a correct calculation of checksum for schedules.</remarks>
        [OperationContract, FaultContract(typeof(FaultException))]
        PersonRequestDto CreateShiftTradeRequest(PersonDto requester, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos);

        /// <summary>
        /// Updates the details for a shift trade request.
        /// </summary>
        /// <param name="personRequestDto">The person request.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="shiftTradeSwapDetailDtos">The details for the shift trade.</param>
        /// <returns></returns>
        [OperationContract, FaultContract(typeof(FaultException))]
        PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos);

        /// <summary>
        /// Updates the message of the person request.
        /// </summary>
        /// <param name="personRequest">The person request.</param>
        /// <returns></returns>
        [OperationContract, FaultContract(typeof(FaultException))]
        PersonRequestDto UpdatePersonRequestMessage(PersonRequestDto personRequest);

        /// <summary>
        /// Accepts the given shift trade request for the SDK user.
        /// </summary>
        /// <param name="personRequest">The person request involving the SDK user.</param>
        /// <returns></returns>
        [OperationContract, FaultContract(typeof(FaultException))]
        PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequest);

        /// <summary>
        /// Denies the given shift trade request for the SDK user.
        /// </summary>
        /// <param name="personRequest">The person request involving the SDK user.</param>
        /// <returns></returns>
        [OperationContract]
        PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequest);

        /// <summary>
        /// Gets all available days off using the given load options.
        /// </summary>
        /// <param name="loadOptionDto">The load options.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<DayOffInfoDto> GetDaysOffs(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets the shift categories using the given load options.
        /// </summary>
        /// <param name="loadOptionDto">The load options.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ShiftCategoryDto> GetShiftCategories(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets the adherence data for the given day and person.
        /// </summary>
        /// <param name="dateTime">The date.</param>
        /// <param name="timeZoneId">The time zone.</param>
        /// <param name="personDto">The SDK user.</param>
        /// <param name="agentPersonDto">The person.</param>
        /// <param name="languageId">The language id. (LCID)</param>
        /// <returns></returns>
        [OperationContract]
        AdherenceDto GetAdherenceData(DateTime dateTime, string timeZoneId, PersonDto personDto,
                                 PersonDto agentPersonDto, int languageId);

		/// <summary>
        /// Gets the queue details between the start and end date for the given person.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <param name="personDto">The person.</param>
        /// <returns></returns>
        [OperationContract]
        IList<AgentQueueStatDetailsDto> GetAgentQueueStatDetails(DateTime startDate, DateTime endDate, string timeZoneId,
                                                          PersonDto personDto);

        /// <summary>
        /// Gets the adherence information between the start and end date for the given person.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <param name="personDto">The person.</param>
        /// <returns></returns>
        [OperationContract]
        IList<AdherenceInfoDto> GetAdherenceInfo(DateTime startDate, DateTime endDate, string timeZoneId,
                                                          PersonDto personDto);

        /// <summary>
        /// Saves the preference.
        /// </summary>
        /// <param name="preferenceRestrictionDto">The preference.</param>
        [OperationContract]
        void SavePreference(PreferenceRestrictionDto preferenceRestrictionDto);

        /// <summary>
        /// Deletes the preference.
        /// </summary>
        /// <param name="preferenceRestrictionDto">The preference.</param>
        [OperationContract]
        void DeletePreference(PreferenceRestrictionDto  preferenceRestrictionDto);

        /// <summary>
        /// Saves the student availability day.
        /// </summary>
        /// <param name="studentAvailabilityDayDto">The student availability day.</param>
        [OperationContract]
        void SaveStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto);

        /// <summary>
        /// Deletes the student availability day.
        /// </summary>
        /// <param name="studentAvailabilityDayDto">The student availability day.</param>
        [OperationContract]
        void DeleteStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto);

        /// <summary>
        /// Saves the template for extended preferences for the SDK user.
        /// </summary>
        /// <param name="extendedPreferenceTemplateDto">The extended preferences template.</param>
        [OperationContract, FaultContract(typeof(FaultException))]
        void SaveExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto);

        /// <summary>
        /// Deletes the template for extended preferences for the SDK user.
        /// </summary>
        /// <param name="extendedPreferenceTemplateDto">The extended preferences template.</param>
        [OperationContract, FaultContract(typeof(FaultException))]
        void DeleteExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto);

        /// <summary>
        /// Gets the extended preference templates for the given person.
        /// </summary>
        /// <param name="personDto">The person.</param>
        /// <returns></returns>
        [OperationContract, FaultContract(typeof(FaultException))]
        ICollection<ExtendedPreferenceTemplateDto> GetExtendedPreferenceTemplates(PersonDto personDto);

        /// <summary>
        /// Gets the validated schedule details with restrictions and schedule period information for the schedule period containing the given date.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="dateInPeriod">The date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriod(PersonDto person, DateOnlyDto dateInPeriod, string timeZoneId);

        /// <summary>
        /// Gets the validated schedule details with restrictions and schedule period information for the schedule period containing the given date.
        /// </summary>
        /// <param name="queryDto">The query.</param>
        [OperationContract]
        ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriodByQuery(QueryDto queryDto);

        /// <summary>
        /// Gets overtime and shift allowance details for a person between the given dates.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPerson(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate);

        /// <summary>
        /// Gets overtime and shift allowance details for people between the given dates.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="timeZoneId">The time zone id.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPersons(PersonDto[] personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId);

        [OperationContract(AsyncPattern = true), Obsolete("Support for this functionality will be removed.")]
        IAsyncResult BeginCreateServerScheduleDistribution(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Gets the public notes between the given dates using the load options.
        /// </summary>
        /// <param name="publicNoteLoadOptionDto">The load options.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        [OperationContract, FaultContract(typeof(FaultException))]
        ICollection<PublicNoteDto> GetPublicNotes(PublicNoteLoadOptionDto publicNoteLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate);

        /// <summary>
        /// Saves a public note.
        /// </summary>
        /// <param name="publicNoteDto">The public note.</param>
        /// <remarks>This can be used to other information that should be available to the agents (for example seat information).</remarks>
        [OperationContract, FaultContract(typeof(FaultException))]
        void SavePublicNote(PublicNoteDto publicNoteDto);

        /// <summary>
        /// Deletes the public note.
        /// </summary>
        /// <param name="publicNoteDto"></param>
        [OperationContract]
        void DeletePublicNote(PublicNoteDto publicNoteDto);

        /// <summary>
        /// Gets the planning time bank details for the given person and date.
        /// </summary>
        /// <param name="personDto">The person.</param>
        /// <param name="dateOnlyDto">The date.</param>
        /// <returns></returns>
        [OperationContract]
        PlanningTimeBankDto GetPlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto);

        /// <summary>
        /// Saves the preferred balance out for the period containing the given date.
        /// </summary>
        /// <param name="personDto">The person.</param>
        /// <param name="dateOnlyDto">The date.</param>
        /// <param name="balanceOutMinute">The preferred balance out by the end of the period in minutes.</param>
        [OperationContract]
        void SavePlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto, int balanceOutMinute);

		[Obsolete("Support for this functionality will be removed.")]
		void EndCreateServerScheduleDistribution(IAsyncResult result);

		/// <summary>
		/// Get the schedules for the given query.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		[OperationContract]
    	ICollection<SchedulePartDto> GetSchedulesByQuery(QueryDto queryDto);

		/// <summary>
		/// Get the multiplicators for the given query.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		[OperationContract]
		ICollection<MultiplicatorDto> GetMultiplicatorsByQuery(QueryDto queryDto);

		/// <summary>
		/// Get the multiplicator definition set for the given query.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		[OperationContract]
		ICollection<DefinitionSetDto> GetMultiplicatorDefinitionSetByQuery(QueryDto queryDto);

		/// <summary>
		/// Gets the Schedule Tags for the given query.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		[OperationContract]
		ICollection<ScheduleTagDto> GetScheduleTagByQuery(QueryDto queryDto);

		/// <summary>
		/// Gets all schedule changes from a certain changing point in time.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		/// <returns></returns>
		[OperationContract]
		ScheduleChangesDto GetSchedulesByChangedDateTime(QueryDto queryDto);

		/// <summary>
		/// Queries the system after saved settings for the schedule change subscriptions.
		/// </summary>
		/// <param name="queryDto">The query.</param>
		/// <returns>A collection with one item with the saved subscriptions containing the different listeners.</returns>
		[OperationContract, FaultContract(typeof(FaultException))]
		ICollection<ScheduleChangesSubscriptionsDto> GetScheduleChangeSubscriptionsByQuery(QueryDto queryDto);
	}
}