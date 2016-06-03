using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodTransformer
	{
		private static readonly DateTime eternity = new DateTime(2059, 12, 31);
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly IAnalyticsPersonPeriodMapNotDefined _analyticsPersonPeriodMapNotDefined;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private readonly ICommonNameDescriptionSetting _commonNameDescription;

		public PersonPeriodTransformer(
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, 
			IAnalyticsSkillRepository analyticsSkillRepository, 
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsTeamRepository analyticsTeamRepository, 
			IAnalyticsPersonPeriodMapNotDefined analyticsPersonPeriodMapNotDefined, 
			IAnalyticsDateRepository analyticsDateRepository, 
			IAnalyticsTimeZoneRepository analyticsTimeZoneRepository, 
			ICommonNameDescriptionSetting commonNameDescription)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsTeamRepository = analyticsTeamRepository;
			_analyticsPersonPeriodMapNotDefined = analyticsPersonPeriodMapNotDefined;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_commonNameDescription = commonNameDescription;
		}

		public AnalyticsPersonPeriod Transform(IPerson person, IPersonPeriod personPeriod, out List<AnalyticsSkill> analyticsSkills)
		{
			var businessUnitId =
				MapBusinessId(person.PersonPeriodCollection.First().Team.BusinessUnitExplicit.Id.GetValueOrDefault());
			var siteId = MapSiteId(businessUnitId, personPeriod.Team.Site.Id.GetValueOrDefault(),
				personPeriod.Team.Site.Description.Name);
			var teamId = MapTeamId(personPeriod.Team.Id.GetValueOrDefault(), siteId,
				personPeriod.Team.Description.Name, businessUnitId);
			var skillsetId = MapSkillsetId(
				personPeriod.PersonSkillCollection.Select(a => a.Skill.Id.GetValueOrDefault()).ToList(),
				businessUnitId, _analyticsPersonPeriodMapNotDefined, out analyticsSkills);
			
			var analyticsPersonPeriod = new AnalyticsPersonPeriod
			{
				PersonCode = person.Id.GetValueOrDefault(),
				PersonPeriodCode = personPeriod.Id.GetValueOrDefault(),
				PersonName = GetPersonName(person),
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				EmploymentNumber = person.EmploymentNumber,
				EmploymentTypeCode = null,
				EmploymentTypeName = personPeriod.PersonContract.Contract.EmploymentType.ToString(),
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
				IsAgent = person.IsAgent(DateOnly.Today),
				IsUser = false,
				DatasourceId = 1,
				DatasourceUpdateDate = person.UpdatedOn.GetValueOrDefault(),
				ToBeDeleted = false,
				WindowsDomain = "",
				WindowsUsername = "" // WindowsDomain and WindowsUsername are filled in by nightly step.
			};

			analyticsPersonPeriod = FixDatesAndInterval(
				analyticsPersonPeriod,
				personPeriod.Period.StartDate.Date,
				personPeriod.Period.EndDate.Date,
				person.PermissionInformation.DefaultTimeZone());

			return analyticsPersonPeriod;
		}

		public string GetPersonName(IPerson person)
		{
			return _commonNameDescription.BuildCommonNameDescription(person);
		}

		public AnalyticsPersonPeriod FixDatesAndInterval(AnalyticsPersonPeriod analyticsPersonPeriod,
			DateTime personPeriodStartDate, DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo)
		{
			var timeZoneId = MapTimeZoneId(timeZoneInfo.Id);

			var maxDate = MapMaxDate();
			var minDate = _analyticsDateRepository.MinDate().DateDate;

			var validFromDate = ValidFromDate(personPeriodStartDate, timeZoneInfo, minDate);
			var validToDate = ValidToDate(personPeriodEndDate, timeZoneInfo, maxDate.DateDate);

			var validFromDateId = MapDateId(validFromDate);
			var validToDateId = MapDateId(validToDate);
			var validToDateIdMaxDate = GetValidToDateIdMaxDate(validToDate, maxDate, validToDateId);

			var validFromDateLocal = personPeriodStartDate < minDate ? minDate : personPeriodStartDate;
			var validToDateLocal = ValidToDateLocal(personPeriodEndDate, maxDate);

			var validFromDateIdLocal = MapDateId(validFromDateLocal);
			var validToDateIdLocal = ValidToDateIdLocal(MapDateId(validToDateLocal), maxDate);

			var intervalsPerDay = _analyticsPersonPeriodRepository.IntervalsPerDay();
			var validFromIntervalId = ValidFromIntervalId(validFromDate, intervalsPerDay);
			var validToIntervalId = ValidToIntervalId(validToDate, intervalsPerDay);

			var validToIntervalIdMaxDate = getValidToIntervalIdMaxDate(validToIntervalId, validToDateId);

			analyticsPersonPeriod.TimeZoneId = timeZoneId;

			analyticsPersonPeriod.ValidFromDate = validFromDate; // UTC tid
			analyticsPersonPeriod.ValidToDate = validToDate; // UTC tid
			analyticsPersonPeriod.ValidFromDateId = validFromDateId; // UTC tid id
			analyticsPersonPeriod.ValidToDateId = validToDateId; // UTC tid id
			analyticsPersonPeriod.ValidFromIntervalId = validFromIntervalId; // UTC interval
			analyticsPersonPeriod.ValidToIntervalId = validToIntervalId; // UTC interval 

			analyticsPersonPeriod.EmploymentStartDate = validFromDate; // UTC tid
			analyticsPersonPeriod.EmploymentEndDate = validToDate; // UTC tid

			analyticsPersonPeriod.ValidToDateIdMaxDate = validToDateIdMaxDate;
			analyticsPersonPeriod.ValidToIntervalIdMaxDate = validToIntervalIdMaxDate;

			analyticsPersonPeriod.ValidFromDateIdLocal = validFromDateIdLocal;
			analyticsPersonPeriod.ValidToDateIdLocal = validToDateIdLocal;
			analyticsPersonPeriod.ValidFromDateLocal = validFromDateLocal;
			analyticsPersonPeriod.ValidToDateLocal = validToDateLocal;

			return analyticsPersonPeriod;
		}

		public static DateTime ValidToDateLocal(DateTime personPeriodEndDate, IAnalyticsDate maxDate)
		{
			if (personPeriodEndDate.Equals(eternity))
				return maxDate.DateDate;
			return personPeriodEndDate;
		}

		public static int ValidToDateIdLocal(int dateId, IAnalyticsDate maxDate)
		{
			if (dateId == -2)
				return maxDate.DateId;
			return dateId;
		}

		public static int ValidToIntervalId(DateTime validToDate, int intervalsPerDay)
		{
			return new IntervalBase(getPeriodIntervalEndDate(validToDate, intervalsPerDay), intervalsPerDay).Id;
		}

		public static int ValidFromIntervalId(DateTime validFromDate, int intervalsPerDay)
		{
			return new IntervalBase(validFromDate, intervalsPerDay).Id;
		}

		private int getValidToIntervalIdMaxDate(int validToIntervalId, int validToDateId)
		{
			// Samma som ValidToIntervalId om inte validToDateId är eterntity då ska det vara sista interval i dim_interval
			if (validToDateId != -2)
				return validToIntervalId;
			return _analyticsPersonPeriodRepository.MaxIntervalId();
		}

		private static DateTime getPeriodIntervalEndDate(DateTime endDate, int intervalsPerDay)
		{
			if (endDate.Equals(eternity))
			{
				return endDate;
			}

			var minutesPerInterval = 1440 / intervalsPerDay;
			return endDate.AddMinutes(-minutesPerInterval);
		}

		public static int GetValidToDateIdMaxDate(DateTime validToDate, IAnalyticsDate maxDate, int validToDateId)
		{
			// Samma som ValidToDateId om inte eternity då ska vara näst sista dagen i dim_date
			return validToDate.Equals(eternity)
				? maxDate.DateId - 1
				: validToDateId;
		}

		public static DateTime ValidToDate(DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo, DateTime maxDate)
		{
			var validToDate = personPeriodEndDate.Equals(eternity) || personPeriodEndDate > maxDate
				? eternity
				: timeZoneInfo.SafeConvertTimeToUtc(personPeriodEndDate.AddDays(1));
			// Add one days because there is no end time in app database but it is in analytics and we do not want gap between person periods end and start date.
			return validToDate;
		}

		public static DateTime ValidFromDate(DateTime personPeriodStartDate, TimeZoneInfo timeZoneInfo, DateTime minDate)
		{
			if (personPeriodStartDate < minDate)
				return minDate;
			var validFromDate = timeZoneInfo.SafeConvertTimeToUtc(personPeriodStartDate);
			if (validFromDate >= eternity)
				validFromDate = eternity;
			return validFromDate;
		}

		public int MapDateId(DateTime date)
		{
			var analyticsDate = _analyticsDateRepository.Date(date);
			if (analyticsDate != null)
				return analyticsDate.DateId;
			return -1;
		}

		public IAnalyticsDate MapMaxDate()
		{
			return _analyticsDateRepository.MaxDate();
		}

		public int? MapTimeZoneId(string timeZoneCode)
		{
			var timeZone = _analyticsTimeZoneRepository.Get(timeZoneCode);
			return timeZone?.TimeZoneId;
		}

		public int MapTeamId(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return _analyticsTeamRepository.GetOrCreate(teamCode, siteId, teamName, businessUnitId);
		}

		public int MapBusinessId(Guid businessUnitCode)
		{
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitCode);
			return businessUnit.BusinessUnitId;
		}

		public int MapSiteId(int businessUnitId, Guid siteCode, string siteName)
		{
			return _analyticsPersonPeriodRepository.SiteId(siteCode, siteName, businessUnitId);
		}

		public static AnalyticsSkillSet NewSkillSetFromSkills(List<AnalyticsSkill> skills)
		{
			var skillSetCode = string.Join(",", skills.OrderBy(a => a.SkillId).Select(a => a.SkillId));
			var skillSetName = string.Join(",", skills.OrderBy(a => a.SkillId).Select(a => a.SkillName));

			var newSkillSet = new AnalyticsSkillSet
			{
				SkillsetCode = skillSetCode,
				SkillsetName = skillSetName,
				BusinessUnitId = skills.First().BusinessUnitId,
				DatasourceId = skills.First().DatasourceId,
				DatasourceUpdateDate = skills.Max(a => a.DatasourceUpdateDate)
			};
			return newSkillSet;
		}

		public int MapSkillsetId(List<Guid> applicationSkillCodes, int businessUnitId, IAnalyticsPersonPeriodMapNotDefined analyticsPersonPeriodMapNotDefined)
		{
			List<AnalyticsSkill> listOfSkills;
			return MapSkillsetId(applicationSkillCodes, businessUnitId, analyticsPersonPeriodMapNotDefined, out listOfSkills);
		}

		// Map skills to skillset. If not exists it will create it.
		public int MapSkillsetId(List<Guid> applicationSkillCodes, int businessUnitId, IAnalyticsPersonPeriodMapNotDefined analyticsPersonPeriodMapNotDefined, out List<AnalyticsSkill> mappedAnalyticsSkills)
		{
			mappedAnalyticsSkills = null;
			if (applicationSkillCodes.IsEmpty())
				return -1;

			var allAnalyticsSkills = _analyticsSkillRepository.Skills(businessUnitId).ToList();
			mappedAnalyticsSkills = allAnalyticsSkills.Where(a => applicationSkillCodes.Contains(a.SkillCode)).ToList();

			if (allAnalyticsSkills.Count(a => applicationSkillCodes.Contains(a.SkillCode)) != applicationSkillCodes.Count)
			{
				// Skill exists in app but not yet in analytics
				return analyticsPersonPeriodMapNotDefined.MaybeThrowErrorOrNotDefined(
					$"Some skill missing in analytics = [{string.Join(", ", applicationSkillCodes.Where(a => allAnalyticsSkills.All(b => a != b.SkillCode)))}]");
			}
			
			var skillSetId = _analyticsSkillRepository.SkillSetId(mappedAnalyticsSkills);

			if (skillSetId.HasValue)
				return skillSetId.Value;

			// Create new skill set (combination of skills)
			var newSkillSet = NewSkillSetFromSkills(mappedAnalyticsSkills);
			_analyticsSkillRepository.AddSkillSet(newSkillSet);
			var newSkillSetId = _analyticsSkillRepository.SkillSetId(mappedAnalyticsSkills);

			if (!newSkillSetId.HasValue) // If something got wrong anyways
				return analyticsPersonPeriodMapNotDefined.MaybeThrowErrorOrNotDefined("Error when creating new skills set");

			var newBridgeSkillSetSkills = NewBridgeSkillSetSkillsFromSkills(mappedAnalyticsSkills, newSkillSetId.Value);
			foreach (var bridgeSkillSetSkill in newBridgeSkillSetSkills)
			{
				_analyticsSkillRepository.AddBridgeSkillsetSkill(bridgeSkillSetSkill);
			}
			return newSkillSetId.Value;
		}

		public static IEnumerable<AnalyticsBridgeSkillsetSkill> NewBridgeSkillSetSkillsFromSkills(List<AnalyticsSkill> listOfSkills, int newSkillSetId)
		{
			return listOfSkills.Select(skill => new AnalyticsBridgeSkillsetSkill
			{
				SkillsetId = newSkillSetId,
				SkillId = skill.SkillId,
				BusinessUnitId = skill.BusinessUnitId,
				DatasourceId = skill.DatasourceId,
				DatasourceUpdateDate = skill.DatasourceUpdateDate
			});
		}
	}
}