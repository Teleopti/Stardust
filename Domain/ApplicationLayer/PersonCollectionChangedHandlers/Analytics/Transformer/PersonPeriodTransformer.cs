using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer
{
	public class PersonPeriodTransformer
	{
		private IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private IPersonRepository _personRepository;
		private static DateTime Eternity = new DateTime(2059, 12, 30);

		public PersonPeriodTransformer(IPersonRepository personRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			this._personRepository = personRepository;
			this._analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		public AnalyticsPersonPeriod Transform(IPerson person, IPersonPeriod personPeriod)
		{
			var businessUnitId = MapBusinessId(person.PersonPeriodCollection.First().Team.BusinessUnitExplicit.Id.GetValueOrDefault());
			var siteId = MapSiteId(businessUnitId, personPeriod.Team.Site.Id.GetValueOrDefault(),
						personPeriod.Team.Site.Description.Name);
			var teamId = MapTeamId(personPeriod.Team.Id.GetValueOrDefault(), siteId,
				personPeriod.Team.Description.Name, businessUnitId);
			var skillsetId = MapSkillsetId(
					personPeriod.PersonSkillCollection.Select(a => a.Skill.Id.GetValueOrDefault()).ToList(),
					businessUnitId);

			var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
			var timeZoneId = MapTimeZoneId(person.PermissionInformation.DefaultTimeZone().Id);

			var validFromDate = ValidFromDate(personPeriod, timeZoneInfo);
			var validToDate = ValidToDate(personPeriod, timeZoneInfo);

			var maxDate = MapMaxDate();
			var validFromDateId = MapDateId(validFromDate);
			var validToDateId = MapDateId(validToDate);
			int validToDateIdMaxDate = GetValidToDateIdMaxDate(validToDate, maxDate, validToDateId);

			var validFromDateLocal = personPeriod.Period.StartDate.Date;
			var validToDateLocal = personPeriod.Period.EndDate.Date;

			var validFromDateIdLocal = MapDateId(validFromDateLocal);
			var validToDateIdLocal = MapDateId(validToDateLocal);

			var analyticsPersonPeriod = new AnalyticsPersonPeriod()
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonPeriodCode = personPeriod.Id.GetValueOrDefault(),
				PersonName = person.Name.ToString(),
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				EmploymentNumber = person.EmploymentNumber,
				EmploymentTypeCode = -1, // TODO 
				EmploymentTypeName = "Not Defined?", // TODO
				ContractCode = personPeriod.PersonContract.Contract.Id.GetValueOrDefault(),
				ContractName = personPeriod.PersonContract.Contract.Description.Name,
				ParttimeCode = personPeriod.PersonContract.PartTimePercentage.Id.GetValueOrDefault(),
				ParttimePercentage = personPeriod.PersonContract.PartTimePercentage.Percentage.Value * 100 + "%",
				TeamId = teamId,
				TeamCode = personPeriod.Team.Id.GetValueOrDefault(),
				TeamName = personPeriod.Team.Description.Name,
				SiteId = siteId,
				SiteCode = personPeriod.Team.Site.Id.GetValueOrDefault(),
				SiteName = personPeriod.Team.Site.Description.Name,
				BusinessUnitId = businessUnitId,
				BusinessUnitCode = personPeriod.Team.BusinessUnitExplicit.Id.GetValueOrDefault(),
				BusinessUnitName = personPeriod.Team.BusinessUnitExplicit.Name,
				SkillsetId = skillsetId,
				Email = person.Email,
				Note = person.Note,
				EmploymentStartDate = personPeriod.Period.StartDate.Date,
				EmploymentEndDate = personPeriod.Period.EndDate.Date,
				TimeZoneId = timeZoneId,
				IsAgent = person.IsAgent(DateOnly.Today),
				IsUser = false,
				DatasourceId = 1,
				InsertDate = DateTime.Now, // TODO updateringen borde sköta detta
				UpdateDate = DateTime.Now, // TODO updateringen borde sköta detta
				DatasourceUpdateDate = person.UpdatedOn.GetValueOrDefault(),
				ToBeDeleted = false,
				WindowsDomain = "",     // TODO
				WindowsUsername = "",   // TODO

				ValidFromDate = validFromDate, // UTC tid
				ValidToDate = validToDate,     // UTC tid
				ValidFromDateId = validFromDateId,   // UTC tid id
				ValidToDateId = validToDateId,     // UTC tid id
				ValidFromIntervalId = -1,   // TODO UTC interval
				ValidToIntervalId = -1,     // TODO UTC interval 

				ValidToDateIdMaxDate = validToDateIdMaxDate,  // Samma som ValidToDateId om inte eternity då ska vara sista dagen i dim_date
				ValidToIntervalIdMaxDate = -1, // Samma som ValidToIntervalId om inte eterntity då ska det vara sista interval i dim_interval

				ValidFromDateIdLocal = validFromDateIdLocal,
				ValidToDateIdLocal = validToDateIdLocal,
				ValidFromDateLocal = validFromDateLocal,
				ValidToDateLocal = validToDateLocal
			};

			return analyticsPersonPeriod;
		}

		private static int GetValidToDateIdMaxDate(DateTime validToDate, IAnalyticsDate maxDate, int validToDateId)
		{
			// Samma som ValidToDateId om inte eternity då ska vara sista dagen i dim_date
			return validToDate.Equals(Eternity)
				? maxDate.DateId
				: validToDateId;
		}

		private static DateTime ValidToDate(IPersonPeriod personPeriod, TimeZoneInfo timeZoneInfo)
		{
			DateTime validToDate = personPeriod.EndDate().Date.Equals(Eternity)
				? Eternity
				: timeZoneInfo.SafeConvertTimeToUtc(personPeriod.EndDate().Date.AddDays(1));
			return validToDate;
		}

		private static DateTime ValidFromDate(IPersonPeriod personPeriod, TimeZoneInfo timeZoneInfo)
		{
			var validFromDate = timeZoneInfo.SafeConvertTimeToUtc(personPeriod.StartDate.Date);
			if (validFromDate >= Eternity)
				validFromDate = Eternity;
			return validFromDate;
		}

		private int MapDateId(DateTime date)
		{
			return _analyticsPersonPeriodRepository.Date(date).DateId;
		}

		private IAnalyticsDate MapMaxDate()
		{
			return _analyticsPersonPeriodRepository.ValidToMaxDate();
		}

		private int MapTimeZoneId(string timeZoneCode)
		{
			return _analyticsPersonPeriodRepository.TimeZone(timeZoneCode);
		}

		private int MapTeamId(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return _analyticsPersonPeriodRepository.TeamId(teamCode, siteId, teamName, businessUnitId);
		}

		private int MapBusinessId(Guid businessUnitCode)
		{
			return _analyticsPersonPeriodRepository.BusinessUnitId(businessUnitCode);
		}

		private int MapSiteId(int businessUnitId, Guid siteCode, string siteName)
		{
			return _analyticsPersonPeriodRepository.SiteId(siteCode, siteName, businessUnitId);
		}

		private int MapSkillsetId(List<Guid> skillCodes, int businessUnitId)
		{
			// TODO logic for new skills and new combination which should produce a new skillset.

			if (skillCodes.IsEmpty())
				return -1;

			var listOfSkillIds = _analyticsPersonPeriodRepository.Skills(businessUnitId)
				.Where(a => skillCodes.Contains(a.SkillCode)).ToList();
			return _analyticsPersonPeriodRepository.SkillSetId(listOfSkillIds);
		}
	}
}
