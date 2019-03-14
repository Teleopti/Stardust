using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	class FakeTeleoptiSchedulingService : ITeleoptiSchedulingService
	{
		public ICollection<ActivityDto> GetActivities(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<AbsenceDto> GetAbsences(AbsenceLoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public SchedulePartDto GetSchedulePart(PersonDto person, DateOnlyDto startDate, string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<SchedulePartDto> GetScheduleParts(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<SchedulePartDto> GetSchedulePartsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiTimeExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiDetailedExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<PayrollBaseExportDto> GetTeleoptiActivitiesExportData(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId)
		{
			return new List<PayrollBaseExportDto>
				{
					new PayrollBaseExportDto(),
					new PayrollBaseExportDto()
				};
		}

		public ICollection<SchedulePartDto> GetSchedules(ScheduleLoadOptionDto scheduleLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public void SaveSchedulePart(SchedulePartDto schedulePartDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ShiftCategoryDto> GetShiftCategoriesBelongingToRuleSetBag(PersonDto personDto, DateOnlyDto startDateOnlyDto,
			DateOnlyDto endDateOnlyDto)
		{
			throw new NotImplementedException();
		}

		public void DeletePersonRequest(PersonRequestDto personRequestDto)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto SavePersonRequest(PersonRequestDto personRequestDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonRequestDto> GetAllPersonRequests(PersonDto person)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonRequestDto> GetPersonRequests(PersonDto person, DateTime utcStartDate, DateTime utcEndDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonRequestDto> GetAllRequestModifiedWithinPeriodOrPending(PersonDto person, DateTime utcStartDate, DateTime utcEndDate)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto CreateShiftTradeRequest(PersonDto requester, string subject, string message,
			ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message,
			ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto UpdatePersonRequestMessage(PersonRequestDto personRequest)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequest)
		{
			throw new NotImplementedException();
		}

		public PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequest)
		{
			throw new NotImplementedException();
		}

		public ICollection<DayOffInfoDto> GetDaysOffs(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ShiftCategoryDto> GetShiftCategories(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public AdherenceDto GetAdherenceData(DateTime dateTime, string timeZoneId, PersonDto personDto, PersonDto agentPersonDto,
			int languageId)
		{
			throw new NotImplementedException();
		}

		public IList<AgentQueueStatDetailsDto> GetAgentQueueStatDetails(DateTime startDate, DateTime endDate, string timeZoneId, PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public IList<AdherenceInfoDto> GetAdherenceInfo(DateTime startDate, DateTime endDate, string timeZoneId, PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public void SavePreference(PreferenceRestrictionDto preferenceRestrictionDto)
		{
			throw new NotImplementedException();
		}

		public void DeletePreference(PreferenceRestrictionDto preferenceRestrictionDto)
		{
			throw new NotImplementedException();
		}

		public void SaveStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto)
		{
			throw new NotImplementedException();
		}

		public void DeleteStudentAvailabilityDay(StudentAvailabilityDayDto studentAvailabilityDayDto)
		{
			throw new NotImplementedException();
		}

		public void SaveExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto)
		{
			throw new NotImplementedException();
		}

		public void DeleteExtendedPreferenceTemplate(ExtendedPreferenceTemplateDto extendedPreferenceTemplateDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ExtendedPreferenceTemplateDto> GetExtendedPreferenceTemplates(PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriod(PersonDto person, DateOnlyDto dateInPeriod, string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public ICollection<ValidatedSchedulePartDto> GetValidatedSchedulePartsOnSchedulePeriodByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPerson(PersonDto person, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorDataForPersons(PersonDto[] personCollection, DateOnlyDto startDate,
			DateOnlyDto endDate, string timeZoneId)
		{
			throw new NotImplementedException();
		}

		public IAsyncResult BeginCreateServerScheduleDistribution(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate,
			string timeZoneId, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public ICollection<PublicNoteDto> GetPublicNotes(PublicNoteLoadOptionDto publicNoteLoadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			throw new NotImplementedException();
		}

		public void SavePublicNote(PublicNoteDto publicNoteDto)
		{
			throw new NotImplementedException();
		}

		public void DeletePublicNote(PublicNoteDto publicNoteDto)
		{
			throw new NotImplementedException();
		}

		public PlanningTimeBankDto GetPlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto)
		{
			throw new NotImplementedException();
		}

		public void SavePlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto, int balanceOutMinute)
		{
			throw new NotImplementedException();
		}

		public void EndCreateServerScheduleDistribution(IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		public ICollection<SchedulePartDto> GetSchedulesByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<MultiplicatorDto> GetMultiplicatorsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<DefinitionSetDto> GetMultiplicatorDefinitionSetByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ScheduleTagDto> GetScheduleTagByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ScheduleChangesSubscriptionsDto> GetScheduleChangeSubscriptionsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ScheduleChangesDto GetSchedulesByChangedDateTime(GetSchedulesByChangeDateQueryDto queryDto)
		{
			throw new NotImplementedException();
		}
	}
}
