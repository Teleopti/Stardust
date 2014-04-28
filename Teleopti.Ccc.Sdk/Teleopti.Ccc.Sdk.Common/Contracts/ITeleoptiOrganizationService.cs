using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Contains operations regarding organisation and other common data.
    /// </summary>
    [ServiceContract(
        Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
        Name = "TeleoptiOrganizationService",
        ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiOrganizationService")]
    public interface ITeleoptiOrganizationService
    {
        /// <summary>
        /// Gets the sites the SDK user has permissions to for the current business unit.
        /// </summary>
        /// <param name="applicationFunction">Application function specifying the purpose of the site listing.</param>
        /// <param name="utcDateTime">The UTC date and time to use in authorization.</param>
        /// <returns>All sites where the SDK user have permission to at least one person in one team.</returns>
        [OperationContract]
        ICollection<SiteDto> GetSites(ApplicationFunctionDto applicationFunction, DateTime utcDateTime);

        /// <summary>
        /// Gets the sites matching the specified query.
        /// </summary>
        /// <param name="queryDto">A predefined query type used to filter out sites.</param>
        /// <returns>The sites matching the specified query.</returns>
        [OperationContract]
        ICollection<SiteDto> GetSitesByQuery(QueryDto queryDto);

        /// <summary>
        /// Gets the teams the SDK user has permissions to for the supplied site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="applicationFunction">Application function specifying the purpose of the site listing.</param>
        /// <param name="utcDateTime">The UTC date and time to use in authorization.</param>
        /// <returns>All teams where the SDK user have permission to at least one person.</returns>
        [OperationContract]
        ICollection<TeamDto> GetTeams(SiteDto site, ApplicationFunctionDto applicationFunction, DateTime utcDateTime);

        /// <summary>
        /// Gets the teams matching the specified query.
        /// </summary>
        /// <param name="queryDto">A predefined query type to filer out teams.</param>
        /// <returns>The teams matching the specified query.</returns>
        [OperationContract]
        ICollection<TeamDto> GetTeamsByQuery(QueryDto queryDto);

        /// <summary>
        /// Gets all the teams where SDK user has permissions to at least one person.
        /// </summary>
        /// <param name="applicationFunction">Application function specifying the purpose of the site listing.</param>
        /// <param name="utcDateTime">The UTC date and time to use in authorization.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<TeamDto> GetAllPermittedTeams(ApplicationFunctionDto applicationFunction, DateTime utcDateTime);

        /// <summary>
        /// Gets all the persons for the supplied team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="applicationFunction">Application function specifying the purpose of the site listing.</param>
        /// <param name="utcDateTime">The UTC date and time to use in authorization and team membership check.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonDto> GetPersonsByTeam(TeamDto team, ApplicationFunctionDto applicationFunction, DateTime utcDateTime);

        /// <summary>
        /// Gets the team members of the given person at date.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="utcDate">The date.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonDto> GetPersonTeamMembers(PersonDto person, DateTime utcDate);

        /// <summary>
        /// Gets all the people the SDK user has permission to.
        /// </summary>
        /// <param name="excludeLoggedOnPerson">True to exclude the SDK user.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PersonDto> GetPersons(bool excludeLoggedOnPerson);

        /// <summary>
        /// Gets all the people with a specific query specified.
        /// </summary>
        /// <param name="queryDto">A predefined query type used to filter out persons.</param>
        /// <returns>A list of found persons according to the query.</returns>
        [OperationContract]
        ICollection<PersonDto> GetPersonsByQuery(QueryDto queryDto);

        /// <summary>
        /// Gets the person periods intersecting the given start and end date.
        /// </summary>
        /// <param name="loadOptionDto">The options for loading person periods.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
    	[OperationContract]
    	ICollection<PersonPeriodDetailDto> GetPersonPeriods(PersonPeriodLoadOptionDto loadOptionDto, DateOnlyDto startDate, DateOnlyDto endDate);

        /// <summary>
        /// Saves a new person.
        /// </summary>
        /// <param name="personDto">The person details.</param>
        [OperationContract]
        void SavePerson(PersonDto personDto);

        /// <summary>
        /// Gets the team of the SDK user for the given date.
        /// </summary>
        /// <param name="utcDate">The date.</param>
        /// <returns></returns>
        [OperationContract]
        TeamDto GetLoggedOnPersonTeam(DateTime utcDate);

        /// <summary>
        /// Gets the push message dialogue.
        /// </summary>
        /// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
        /// <returns></returns>
        [OperationContract]
        PushMessageDialogueDto GetPushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto);

        /// <summary>
        /// Gets the push message dialogues unreplied for specified person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PushMessageDialogueDto> GetPushMessageDialoguesNotRepliedTo(PersonDto person);

        /// <summary>
        /// Saves the push message dialogue.
        /// </summary>
        /// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
        [OperationContract]
        void SavePushMessageDialogue(PushMessageDialogueDto pushMessageDialogueDto);

        /// <summary>
        /// Sets the dialogue reply.
        /// </summary>
        /// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
        /// <param name="dialogueReply">The dialogue reply.</param>
        /// <param name="sender">The sender.</param>
        [OperationContract]
        void SetDialogueReply(PushMessageDialogueDto pushMessageDialogueDto, string dialogueReply, PersonDto sender);

        /// <summary>
        /// Sets the reply.
        /// </summary>
        /// <param name="pushMessageDialogueDto">The push message dialogue dto.</param>
        /// <param name="reply">The reply.</param>
        [OperationContract]
        void SetReply(PushMessageDialogueDto pushMessageDialogueDto, string reply);

        /// <summary>
        /// Gets the person accounts.
        /// </summary>
        /// <param name="person">The person id.</param>
        /// <param name="containingDate">The containing date.</param>
        [OperationContract]
        ICollection<PersonAccountDto> GetPersonAccounts(PersonDto person, DateOnlyDto containingDate);

        /// <summary>
        /// Gets the overtime and shift allowance for the given person between the start and end date.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="utcStartTime">The start date.</param>
        /// <param name="utcEndTime">The end date.</param>
        /// <returns></returns>
        [Obsolete("This method should be called from the scheduling service instead. This is just for compatibility with old payroll exports.")]
        [OperationContract]
        ICollection<MultiplicatorDataDto> GetPersonMultiplicatorData(PersonDto person, DateTime utcStartTime, DateTime utcEndTime);

        /// <summary>
        /// Gets the person skill periods for persons.
        /// </summary>
        /// <param name="personList">The person list.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A collection of PersonSkillPeriodDto which contains available skills for a person during a period of time. Multiple items per person can be returned.</returns>
        [OperationContract]
        ICollection<PersonSkillPeriodDto> GetPersonSkillPeriodsForPersons(PersonDto[] personList, DateOnlyDto startDate, DateOnlyDto endDate);

        /// <summary>
        /// Gets the contracts.
        /// </summary>
        /// <param name="loadOptionDto">The options for loading.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ContractDto> GetContracts(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets the part time percentages.
        /// </summary>
        /// <param name="loadOptionDto">The options for loading.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PartTimePercentageDto> GetPartTimePercentages(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Gets the overtime definitions.
        /// </summary>
        /// <param name="loadOptionDto">The options for loading.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<OvertimeDefinitionSetDto> GetOvertimeDefinitions(LoadOptionDto loadOptionDto);


        /// <summary>
        /// Gets the contract schedule definitions.
        /// </summary>
        /// <param name="loadOptionDto">The options for loading.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<ContractScheduleDto> GetContractSchedules(LoadOptionDto loadOptionDto);

        /// <summary>
        /// Adds the person to Teleopti CCC.
        /// </summary>
        /// <param name="personDto">The person details.</param>
        [OperationContract]
        void AddPerson(PersonDto personDto);

        /// <summary>
        /// Updates a person in Teleopti CCC.
        /// </summary>
        /// <param name="personDto">The person details.</param>
        [OperationContract, FaultContract(typeof(FaultException))]
        void UpdatePerson(PersonDto personDto);

        /// <summary>
        /// Adds a person period to a person.
        /// </summary>
        /// <param name="personDto">The person dto.</param>
        /// <param name="personPeriodDto">The person period dto.</param>
        [OperationContract, FaultContract(typeof(FaultException))]
        void AddPersonPeriod(PersonDto personDto, PersonPeriodDto personPeriodDto);

        /// <summary>
        /// Gets the sites on given business unit.
        /// </summary>
        /// <param name="businessUnitDto">The business unit dto.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<SiteDto> GetSitesOnBusinessUnit(BusinessUnitDto businessUnitDto);

        /// <summary>
        /// Gets the teams on the given site.
        /// </summary>
        /// <param name="siteDto">The site dto.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnSite"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnSite"), OperationContract]
        ICollection<TeamDto> GetTeamsOnSite(SiteDto siteDto);

        /// <summary>
        /// Sets write protection details for one person.
        /// </summary>
        /// <param name="personWriteProtectionDto">The write protection details.</param>
        [OperationContract, FaultContract(typeof(FaultException))]
        void SetWriteProtectionDateOnPerson(PersonWriteProtectionDto personWriteProtectionDto);

        /// <summary>
        /// Gets the write protection details for one person.
        /// </summary>
        /// <param name="personDto"></param>
        /// <returns></returns>
        [OperationContract]
        PersonWriteProtectionDto GetWriteProtectionDateOnPerson(PersonDto personDto);

        /// <summary>
        /// Gets the available payroll exports for the given query.
        /// </summary>
        /// <param name="queryDto"></param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PayrollExportDto> GetPayrollExportByQuery(QueryDto queryDto);

        /// <summary>
        /// Get the payroll result status by the given query.
        /// </summary>
        /// <param name="queryDto"></param>
        /// <returns></returns>
        [OperationContract]
        ICollection<PayrollResultDto> GetPayrollResultStatusByQuery(QueryDto queryDto);

        /// <summary>
        /// Get the available group pages for the given query.
        /// </summary>
        /// <param name="queryDto"></param>
        /// <returns></returns>
		[OperationContract]
    	ICollection<GroupPageDto> GroupPagesByQuery(QueryDto queryDto);

        /// <summary>
        /// Get the groups available for the given query.
        /// </summary>
        /// <param name="queryDto"></param>
        /// <returns></returns>
		[OperationContract]
    	ICollection<GroupPageGroupDto> GroupPageGroupsByQuery(QueryDto queryDto);

        /// <summary>
        /// Get business units by specified query.
        /// </summary>
        /// <param name="queryDto">The query object derived from QueryDto.</param>
        /// <returns>The business units.</returns>
        [OperationContract]
        ICollection<BusinessUnitDto> GetBusinessUnitsByQuery(QueryDto queryDto);

		/// <summary>
		/// Get the scenarios for the given query.
		/// </summary>
		/// <param name="queryDto"></param>
		/// <returns></returns>
		[OperationContract]
		ICollection<ScenarioDto> GetScenariosByQuery(QueryDto queryDto);

		/// <summary>
		/// Get the person optional values for the given query.
		/// </summary>
		/// <param name="queryDto"></param>
		/// <returns></returns>
		[OperationContract]
		ICollection<PersonOptionalValuesDto> GetPersonOptionalValuesByQuery(QueryDto queryDto);

		/// <summary>
		/// Sets work time on a person's schedule period belonging to a date.
		/// </summary>
		/// <param name="setSchedulePeriodWorktimeOverrideCommandDto"></param>
		/// <returns></returns>
		[OperationContract]
		CommandResultDto SetSchedulePeriodWorktimeOverride(SetSchedulePeriodWorktimeOverrideCommandDto setSchedulePeriodWorktimeOverrideCommandDto);

		/// <summary>
		/// Gets Application Roles.
		/// </summary>
		/// <param name="queryDto">A predefined query type used.</param>
		/// <returns>Roles.</returns>
		[OperationContract]
		ICollection<RoleDto> GetRolesByQuery(QueryDto queryDto);
    }
}
