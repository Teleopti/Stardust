using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[Serializable]
	public class SdkFakeServiceFactory : ISdkServiceFactory
	{
		public ITeleoptiSchedulingService CreateTeleoptiSchedulingService()
		{
			return new FakeTeleoptiSchedulingService();
		}
		
		public ITeleoptiOrganizationService CreateTeleoptiOrganizationService()
		{
			return new FakeTeleoptiOrganizationService();
		}

		public IPayrollExportFeedback CreatePayrollExportFeedback(InterAppDomainArguments intrAppDomainArguments)
		{
			return new FakeServiceBusPayrollExportFeedback(intrAppDomainArguments);
		}
	}

	public class FakeTeleoptiOrganizationService : ITeleoptiOrganizationService
	{
		public ICollection<SiteDto> GetSites(ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			throw new NotImplementedException();
		}

		public ICollection<SiteDto> GetSitesByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<TeamDto> GetTeams(SiteDto site, ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			throw new NotImplementedException();
		}

		public ICollection<TeamDto> GetTeamsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<TeamDto> GetAllPermittedTeams(ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonDto> GetPersonsByTeam(TeamDto team, ApplicationFunctionDto applicationFunction, DateTime utcDateTime)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonDto> GetPersonTeamMembers(PersonDto person, DateTime utcDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonDto> GetPersons(bool excludeLoggedOnPerson)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonDto> GetPersonsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonPeriodDetailDto> GetPersonPeriods(PersonPeriodLoadOptionDto loadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonPeriodDetailDto> GetPersonPeriodsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public void SavePerson(PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public TeamDto GetLoggedOnPersonTeam(DateTime utcDate)
		{
			throw new NotImplementedException();
		}

		public PushMessageDialogueDto GetPushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PushMessageDialogueDto> GetPushMessageDialoguesNotRepliedTo(PersonDto person)
		{
			throw new NotImplementedException();
		}

		public void SavePushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto)
		{
			throw new NotImplementedException();
		}

		public void SetDialogueReply(PushMessageDialogueDto pushMessageDialogueDto, string dialogueReply, PersonDto sender)
		{
			throw new NotImplementedException();
		}

		public void SetReply(PushMessageDialogueDto pushMessageDialogueDto, string reply)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonAccountDto> GetPersonAccounts(PersonDto person, DateOnlyDto containingDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<MultiplicatorDataDto> GetPersonMultiplicatorData(PersonDto person, DateTime utcStartTime, DateTime utcEndTime)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonSkillPeriodDto> GetPersonSkillPeriodsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate)
		{
			throw new NotImplementedException();
		}

		public ICollection<ContractDto> GetContracts(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PartTimePercentageDto> GetPartTimePercentages(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<OvertimeDefinitionSetDto> GetOvertimeDefinitions(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ContractScheduleDto> GetContractSchedules(LoadOptionDto loadOptionDto)
		{
			throw new NotImplementedException();
		}

		public void AddPerson(PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public void UpdatePerson(PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public void AddPersonPeriod(PersonDto personDto, PersonPeriodDto personPeriodDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<SiteDto> GetSitesOnBusinessUnit(BusinessUnitDto businessUnitDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<TeamDto> GetTeamsOnSite(SiteDto siteDto)
		{
			throw new NotImplementedException();
		}

		public void SetWriteProtectionDateOnPerson(PersonWriteProtectionDto personWriteProtectionDto)
		{
			throw new NotImplementedException();
		}

		public PersonWriteProtectionDto GetWriteProtectionDateOnPerson(PersonDto personDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PayrollExportDto> GetPayrollExportByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PayrollResultDto> GetPayrollResultStatusByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<GroupPageDto> GroupPagesByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<GroupPageGroupDto> GroupPageGroupsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<BusinessUnitDto> GetBusinessUnitsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ScenarioDto> GetScenariosByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<PersonOptionalValuesDto> GetPersonOptionalValuesByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public CommandResultDto SetSchedulePeriodWorktimeOverride(
			SetSchedulePeriodWorktimeOverrideCommandDto setSchedulePeriodWorktimeOverrideCommandDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<RoleDto> GetRolesByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public CommandResultDto GrantPersonRole(GrantPersonRoleCommandDto grantPersonRoleCommandDto)
		{
			throw new NotImplementedException();
		}

		public CommandResultDto RevokePersonRole(RevokePersonRoleCommandDto revokePersonRoleCommandDto)
		{
			throw new NotImplementedException();
		}

		public ICollection<ExternalLogOnDto> ExternalLogOnsByQuery(QueryDto queryDto)
		{
			throw new NotImplementedException();
		}

		public CommandResultDto SetPersonExternalLogOn(SetPersonExternalLogOnCommandDto setPersonExternalLogOnCommandDto)
		{
			throw new NotImplementedException();
		}
	}
}
