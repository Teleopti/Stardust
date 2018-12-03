using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class PersonPeriodTransformer : IPersonPeriodTransformer
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsTeamRepository _analyticsTeamRepository;
		private readonly IAnalyticsPersonPeriodMapNotDefined _analyticsPersonPeriodMapNotDefined;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private readonly IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IAnalyticsPersonPeriodDateFixer _analyticsPersonPeriodDateFixer;

		public PersonPeriodTransformer(
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsSkillRepository analyticsSkillRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsTeamRepository analyticsTeamRepository,
			IAnalyticsPersonPeriodMapNotDefined analyticsPersonPeriodMapNotDefined,
			IAnalyticsTimeZoneRepository analyticsTimeZoneRepository,
			IAnalyticsIntervalRepository analyticsIntervalRepository,
			IGlobalSettingDataRepository globalSettingDataRepository,
			IAnalyticsPersonPeriodDateFixer analyticsPersonPeriodDateFixer)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsTeamRepository = analyticsTeamRepository;
			_analyticsPersonPeriodMapNotDefined = analyticsPersonPeriodMapNotDefined;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_analyticsIntervalRepository = analyticsIntervalRepository;
			_globalSettingDataRepository = globalSettingDataRepository;
			_analyticsPersonPeriodDateFixer = analyticsPersonPeriodDateFixer;
		}

		public AnalyticsPersonPeriod Transform(IPerson person, IPersonPeriod personPeriod,
			out List<AnalyticsSkill> analyticsSkills)
		{
			const string notDefined = "Not Defined";

			var businessUnitId =
				MapBusinessId(personPeriod.Team.BusinessUnitExplicit.Id.GetValueOrDefault());
			var siteId = MapSiteId(businessUnitId, personPeriod.Team.Site.Id.GetValueOrDefault(),
				personPeriod.Team.Site.Description.Name);
			var teamId = MapTeamId(personPeriod.Team.Id.GetValueOrDefault(), siteId,
				personPeriod.Team.Description.Name, businessUnitId);
			var skillsetId = MapSkillsetId(
				personPeriod.PersonSkillCollection.Select(a => a.Skill.Id.GetValueOrDefault()).ToList(),
				businessUnitId, out analyticsSkills);

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
				// Could not get logon info within hangfire (It's in Tenant side), so just leave it as "Not Defined".
				// They will be filled in by nightly job (Refer to DimPersonWindowsLoginJobStep).
				WindowsDomain = notDefined,
				WindowsUsername = notDefined
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
			return _globalSettingDataRepository
				.FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting())
				.BuildFor(person);
		}

		public AnalyticsPersonPeriod FixDatesAndInterval(AnalyticsPersonPeriod analyticsPersonPeriod,
			DateTime personPeriodStartDate, DateTime personPeriodEndDate, TimeZoneInfo timeZoneInfo)
		{
			var validFromDate = _analyticsPersonPeriodDateFixer.ValidFromDate(personPeriodStartDate, timeZoneInfo);
			var validToDate = _analyticsPersonPeriodDateFixer.ValidToDate(personPeriodEndDate, timeZoneInfo);

			var validFromDateId = _analyticsPersonPeriodDateFixer.MapDateId(validFromDate);
			var validToDateId = _analyticsPersonPeriodDateFixer.MapDateId(validToDate);
			var validToDateIdMaxDate = _analyticsPersonPeriodDateFixer.GetValidToDateIdMaxDate(validToDate, validToDateId);

			var validFromDateLocal = _analyticsPersonPeriodDateFixer.ValidFromDateLocal(personPeriodStartDate);
			var validToDateLocal = _analyticsPersonPeriodDateFixer.ValidToDateLocal(personPeriodEndDate);

			var validFromDateIdLocal = _analyticsPersonPeriodDateFixer.MapDateId(validFromDateLocal);
			var validToDateIdLocal =
				_analyticsPersonPeriodDateFixer.ValidToDateIdLocal(_analyticsPersonPeriodDateFixer.MapDateId(validToDateLocal));

			var intervalsPerDay = _analyticsIntervalRepository.IntervalsPerDay();
			var validFromIntervalId = _analyticsPersonPeriodDateFixer.ValidFromIntervalId(validFromDate, intervalsPerDay);
			var validToIntervalId = _analyticsPersonPeriodDateFixer.ValidToIntervalId(validToDate, intervalsPerDay);

			var validToIntervalIdMaxDate =
				_analyticsPersonPeriodDateFixer.GetValidToIntervalIdMaxDate(validToIntervalId, validToDateId);

			analyticsPersonPeriod.TimeZoneId = _analyticsTimeZoneRepository.Get(timeZoneInfo.Id)?.TimeZoneId;

			analyticsPersonPeriod.ValidFromDate = validFromDate; // UTC tid
			analyticsPersonPeriod.ValidToDate = validToDate; // UTC tid
			analyticsPersonPeriod.ValidFromDateId = validFromDateId; // UTC tid id
			analyticsPersonPeriod.ValidToDateId = validToDateId; // UTC tid id
			analyticsPersonPeriod.ValidFromIntervalId = validFromIntervalId; // UTC interval
			analyticsPersonPeriod.ValidToIntervalId = validToIntervalId; // UTC interval 

			analyticsPersonPeriod.EmploymentStartDate = validFromDate; // UTC tid
			analyticsPersonPeriod.EmploymentEndDate = validToDate; // UTC tid

			analyticsPersonPeriod.ValidToDateIdMaxDate = validToDateIdMaxDate; // Always date_id >= 0
			analyticsPersonPeriod.ValidToIntervalIdMaxDate = validToIntervalIdMaxDate; // Always date_id >= 0

			analyticsPersonPeriod.ValidFromDateIdLocal = validFromDateIdLocal; // Always date_id >= 0
			analyticsPersonPeriod.ValidToDateIdLocal = validToDateIdLocal; // Always date_id >= 0
			analyticsPersonPeriod.ValidFromDateLocal = validFromDateLocal; // The real date
			analyticsPersonPeriod.ValidToDateLocal = validToDateLocal; // The real date

			return analyticsPersonPeriod;
		}

		public int MapTeamId(Guid teamCode, int siteId, string teamName, int businessUnitId)
		{
			return _analyticsTeamRepository.GetOrCreate(teamCode, siteId, teamName, businessUnitId);
		}

		public int MapBusinessId(Guid businessUnitCode)
		{
			var businessUnit = _analyticsBusinessUnitRepository.Get(businessUnitCode);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			return businessUnit.BusinessUnitId;
		}

		public int MapSiteId(int businessUnitId, Guid siteCode, string siteName)
		{
			return _analyticsPersonPeriodRepository.GetOrCreateSite(siteCode, siteName, businessUnitId);
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

		public int MapSkillsetId(List<Guid> applicationSkillCodes, int businessUnitId)
		{
			return MapSkillsetId(applicationSkillCodes, businessUnitId, out _);
		}

		// Map skills to skillset. If not exists it will create it.
		public int MapSkillsetId(List<Guid> applicationSkillCodes, int businessUnitId,
			out List<AnalyticsSkill> mappedAnalyticsSkills)
		{
			mappedAnalyticsSkills = null;
			if (applicationSkillCodes.IsEmpty())
				return AnalyticsDate.NotDefined.DateId;

			var allAnalyticsSkills = _analyticsSkillRepository.Skills(businessUnitId).ToArray();
			mappedAnalyticsSkills = allAnalyticsSkills.Where(a => applicationSkillCodes.Contains(a.SkillCode)).ToList();

			if (allAnalyticsSkills.Count(a => applicationSkillCodes.Contains(a.SkillCode)) != applicationSkillCodes.Count)
			{
				// Skill exists in app but not yet in analytics
				return _analyticsPersonPeriodMapNotDefined.MaybeThrowErrorOrNotDefined(
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
				return _analyticsPersonPeriodMapNotDefined.MaybeThrowErrorOrNotDefined("Error when creating new skills set");

			var newBridgeSkillSetSkills = NewBridgeSkillSetSkillsFromSkills(mappedAnalyticsSkills, newSkillSetId.Value);
			foreach (var bridgeSkillSetSkill in newBridgeSkillSetSkills)
			{
				_analyticsSkillRepository.AddBridgeSkillsetSkill(bridgeSkillSetSkill);
			}

			return newSkillSetId.Value;
		}

		public static IEnumerable<AnalyticsBridgeSkillsetSkill> NewBridgeSkillSetSkillsFromSkills(
			List<AnalyticsSkill> listOfSkills, int newSkillSetId)
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