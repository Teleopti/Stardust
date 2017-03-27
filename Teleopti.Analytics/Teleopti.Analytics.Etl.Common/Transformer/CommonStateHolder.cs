using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class CommonStateHolder : ICommonStateHolder
	{
		private IList<IPerson> _personCollection;
		private readonly IJobParameters _jobParameters;
		private IList<IScenario> _scenarioCollection;
		private List<ISkillDay> _skillDaysCollection;
		private IScenario _defaultScenario;
		private IList<ISkill> _skillCollection;
		private IList<TimeZoneInfo> _timeZones;
		private IList<TimeZonePeriod> _bridgeTimeZonePeriodList;
		private IList<IPerson> _userCollection;
		private IList<IActivity> _activityCollection;
		private IList<IAbsence> _absenceCollection;
		private readonly ScheduleCacheCollection _scheduleCache = new ScheduleCacheCollection();
		private IList<IDayOffTemplate> _dayOffTemplateCollection;
		private IList<IShiftCategory> _shiftCategoryCollection;
		private IList<IApplicationFunction> _applicationFunctionCollection;
		private IList<IApplicationRole> _applicationRoleCollection;
		private IList<IAvailableData> _availableDataCollection;
		private IList<IContract> _contractCollection;
		private IList<IContractSchedule> _contractScheduleCollection;
		private IList<IPartTimePercentage> _partTimePercentageCollection;
		private IList<IRuleSetBag> _ruleSetBagCollection;
		private IList<IGroupPage> _userDefinedGroupings;
		private IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetCollection;
		private Dictionary<DateTimePeriod, IScheduleDictionary> _dictionaryCache;
		private readonly ConcurrentDictionary<string, ILastChangedReadModel> _timeValues = new ConcurrentDictionary<string, ILastChangedReadModel>();
		private IList<IOptionalColumn> _optionalColumnCollection;
		private CommonStateHolder()
		{
		}

		public CommonStateHolder(IJobParameters jobParameters)
			: this()
		{
			_jobParameters = jobParameters;
		}

		public IList<IScenario> ScenarioCollection
		{
			get { return _scenarioCollection ?? (_scenarioCollection = _jobParameters.Helper.Repository.LoadScenario()); }
		}

		public IList<ISkill> SkillCollection
		{
			get {
				return _skillCollection ?? (_skillCollection = _jobParameters.Helper.Repository.LoadSkill(ActivityCollection));
			}
		}

		public IList<IOptionalColumn> OptionalColumnCollectionAvailableAsGroupPage {
			get
			{
				return _optionalColumnCollection ??
						 (_optionalColumnCollection = _jobParameters.Helper.Repository.LoadOptionalColumnAvailableAsGroupPage());
			}
		}

		public IList<IActivity> ActivityCollection
		{
			get { return _activityCollection ?? (_activityCollection = _jobParameters.Helper.Repository.LoadActivity()); }
		}

		public IList<IAbsence> AbsenceCollection
		{
			get { return _absenceCollection ?? (_absenceCollection = _jobParameters.Helper.Repository.LoadAbsence()); }
		}

		public IList<IShiftCategory> ShiftCategoryCollection
		{
			get {
				return _shiftCategoryCollection ?? (_shiftCategoryCollection = _jobParameters.Helper.Repository.LoadShiftCategory());
			}
		}

		public IList<IDayOffTemplate> DayOffTemplateCollection
		{
			get {
				return _dayOffTemplateCollection ?? (_dayOffTemplateCollection = _jobParameters.Helper.Repository.LoadDayOff());
			}
		}

		public IList<IApplicationFunction> ApplicationFunctionCollection
		{
			get {
				return _applicationFunctionCollection ??
					   (_applicationFunctionCollection = _jobParameters.Helper.Repository.LoadApplicationFunction());
			}
		}

		public IList<IAvailableData> AvailableDataCollection
		{
			get {
				return _availableDataCollection ?? (_availableDataCollection = _jobParameters.Helper.Repository.LoadAvailableData());
			}
		}

		public IList<IScenario> ScenarioCollectionDeletedExcluded
		{
			get
			{
				IList<IScenario> returnList = new List<IScenario>();
				foreach (IScenario scenario in ScenarioCollection)
				{
					IDeleteTag myScenario = scenario as IDeleteTag;
					if (myScenario != null && !myScenario.IsDeleted)
					{
						returnList.Add(scenario);
					}
				}
				return returnList;
			}
		}

		public IList<IApplicationRole> ApplicationRoleCollection
		{
			get {
				return _applicationRoleCollection ??
					   (_applicationRoleCollection = _jobParameters.Helper.Repository.LoadApplicationRole(this));
			}
		}

		public IScenario DefaultScenario
		{
			get {
				return _defaultScenario ?? (_defaultScenario = ScenarioCollectionDeletedExcluded.First(s => s.DefaultScenario));
			}
		}

		public IList<TimeZoneInfo> TimeZoneCollection
		{
			get
			{
				if (_timeZones == null)
				{
					_timeZones = (IList<TimeZoneInfo>)_jobParameters.Helper.Repository.LoadTimeZonesInUse();
					
					// Ensure that the default time zone always exist
					if (!_timeZones.Contains(_jobParameters.DefaultTimeZone))
					{
						_timeZones.Add(_jobParameters.DefaultTimeZone);
					}

					// Ensure that UTC time zone always exist
					TimeZoneInfo utc = TimeZoneInfo.Utc;
					if (!_timeZones.Contains(utc))
					{
						_timeZones.Add(utc);
					}

					// Handle time zones used by data source
					if (_jobParameters.TimeZonesUsedByDataSources == null)
					{
						_jobParameters.TimeZonesUsedByDataSources = _jobParameters.Helper.Repository.LoadTimeZonesInUseByDataSource();
					}
					if (_jobParameters.TimeZonesUsedByDataSources != null)
					{
						foreach (TimeZoneInfo timeZoneInfo in _jobParameters.TimeZonesUsedByDataSources)
						{
							if (!_timeZones.Contains(timeZoneInfo))
							{
								_timeZones.Add(timeZoneInfo);
							}
						}
					}
				}

				return _timeZones;
			}
		}

		public IEnumerable<IPerson> PersonCollection
		{
			get { return _personCollection ?? (_personCollection = _jobParameters.Helper.Repository.LoadPerson(this)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public IList<IPerson> PersonsWithIds(List<Guid> ids)
		{
			return PersonCollection.Where(person => ids.Contains(person.Id.GetValueOrDefault())).ToList();
		}

		public IEnumerable<IContract> ContractCollection
		{
			get { return _contractCollection ?? (_contractCollection = _jobParameters.Helper.Repository.LoadContract()); }
		}

		public IEnumerable<IContractSchedule> ContractScheduleCollection
		{
			get {
				return _contractScheduleCollection ??
					   (_contractScheduleCollection = _jobParameters.Helper.Repository.LoadContractSchedule());
			}
		}

		public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
		{
			get {
				return _partTimePercentageCollection ??
					   (_partTimePercentageCollection = _jobParameters.Helper.Repository.LoadPartTimePercentage());
			}
		}

		public IEnumerable<IRuleSetBag> RuleSetBagCollection
		{
			get { return _ruleSetBagCollection ?? (_ruleSetBagCollection = _jobParameters.Helper.Repository.LoadRuleSetBag()); }
		}

		public IEnumerable<IGroupPage> UserDefinedGroupings(IScheduleDictionary schedules)
		{
			return _userDefinedGroupings ??
						 (_userDefinedGroupings = _jobParameters.Helper.Repository.LoadUserDefinedGroupings());
		}

		public IBusinessUnit BusinessUnit
		{
			get { return ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit; }
		}

		public DateOnlyPeriod SelectedPeriod
		{
			get { return new DateOnlyPeriod(DateOnly.Today, DateOnly.Today); }
		}

		public IScheduleDictionary GetSchedules(DateTimePeriod period, IScenario scenario)
		{
			var scheduleDictionary = _scheduleCache[scenario].GetFromCacheIfAvailable(period);
			if (scheduleDictionary == null)
			{
				scheduleDictionary = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, this);
				_scheduleCache[scenario].Add(period, scheduleDictionary);
			}
			return scheduleDictionary;
		}

		public IDictionary<DateTimePeriod, IScheduleDictionary> GetSchedules(IList<IScheduleChangedReadModel> changed, IScenario scenario)
		{
			_dictionaryCache = new Dictionary<DateTimePeriod, IScheduleDictionary>();

			var groupedChanged = changed.GroupBy(c => c.Date);
			foreach (var changedReadModels in groupedChanged)
			{
				var theDate = changedReadModels.Key;
				// detta måste fixas med tidszon, eller???
				var utcDate = new DateTime(theDate.Date.Ticks, DateTimeKind.Utc);
				var period = new DateTimePeriod(utcDate, utcDate.AddDays(1).AddMilliseconds(-1));
				var personsIds = changedReadModels.Select(scheduleChangedReadModel => scheduleChangedReadModel.Person).ToList();
				var persons = PersonsWithIds(personsIds);
				var scheduleDictionary = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, persons);
				_dictionaryCache.Add(period, scheduleDictionary);
			}

			return _dictionaryCache;
		}

		public IDictionary<DateOnly, IScheduleDictionary> GetSchedules(HashSet<IStudentAvailabilityDay> days, IScenario scenario)
		{
			var dictionary = new Dictionary<DateOnly, IScheduleDictionary>();

			var groupedDays = days.GroupBy(c => c.RestrictionDate);
			foreach (var availabilityDay in groupedDays)
			{
				var theDate = availabilityDay.Key;
				// detta måste fixas med tidszon, eller???
				var utcDate = DateTime.SpecifyKind(theDate.Date, DateTimeKind.Utc);
				var period = new DateTimePeriod(utcDate, utcDate.AddDays(1).AddMilliseconds(-1));
				var personsIds = availabilityDay.Select(d => d.Person.Id.GetValueOrDefault()).ToList();
				var persons = PersonsWithIds(personsIds);
				var scheduleDictionary = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, persons);
				dictionary.Add(theDate, scheduleDictionary);
			}

			return dictionary;
		}

		public IDictionary<DateTimePeriod, IScheduleDictionary> GetScheduleCache()
		{
			return _dictionaryCache ?? (_dictionaryCache = new Dictionary<DateTimePeriod, IScheduleDictionary>());
		}

		public IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScenario scenario)
		{
			return _jobParameters.Helper.Repository.LoadSchedulePartsPerPersonAndDate(period, GetSchedules(period, scenario));
		}

		public ICollection<ISkillDay> GetSkillDaysCollection(IScenario scenario, DateTime lastCheck)
		{
			return _jobParameters.Helper.Repository.LoadSkillDays(scenario, lastCheck).ToArray();
		}

		public ICollection<ISkillDay> GetSkillDaysCollection(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
		{
			// Load skill days in collection from dictionary
			_skillDaysCollection = new List<ISkillDay>();
			foreach (IList<ISkillDay> skillDay in GetSkillDaysDictionary(period, skills, scenario).Values)
			{
				_skillDaysCollection.AddRange(skillDay);
			}

			return _skillDaysCollection;
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> GetSkillDaysDictionary(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
		{
			return _jobParameters.Helper.Repository.LoadSkillDays(period, skills, scenario);
		}

		public IList<IPerson> UserCollection
		{
			get { return _userCollection ?? (_userCollection = _jobParameters.Helper.Repository.LoadUser()); }
		}

		public IList<IScheduleDay> GetSchedulePartPerPersonAndDate(IScheduleDictionary scheduleDictionary)
		{
			var schedulePartCollection = new List<IScheduleDay>();
			DateOnlyPeriod period = scheduleDictionary.Period.VisiblePeriod.ToDateOnlyPeriod(TimeZoneInfo.Local);

			foreach (IPerson person in scheduleDictionary.Keys)
			{
				var personEndDate = person.TerminalDate.GetValueOrDefault(DateOnly.MaxValue);
				var periodToExtract = period;
				if (period.StartDate > personEndDate) continue;
				if (personEndDate < period.EndDate)
				{
					periodToExtract = new DateOnlyPeriod(periodToExtract.StartDate, personEndDate);
				}

				var range = scheduleDictionary[person];
				schedulePartCollection.AddRange(range.ScheduledDayCollection(periodToExtract));
			}

			return schedulePartCollection;
		}

		public IList<IScheduleDay> GetSchedulePartPerPersonAndDate(
			IDictionary<DateTimePeriod, IScheduleDictionary> dictionary)
		{
			var ret = new List<IScheduleDay>();
			foreach (var key in dictionary)
			{
				ret.AddRange(GetSchedulePartPerPersonAndDate(key.Value));
			}
			return ret;
		}

		public IScheduleDay GetSchedulePartOnPersonAndDate(IPerson person, DateOnly restrictionDate, IScenario scenario)
		{
			var period = new DateTimePeriod(DateTime.SpecifyKind(restrictionDate.Date, DateTimeKind.Utc), DateTime.SpecifyKind(restrictionDate.Date, DateTimeKind.Utc).AddDays(1).AddMilliseconds(-1));
			var scheduleCache = GetScheduleCache();
			IScheduleDictionary dic;
			if (scheduleCache.TryGetValue(period, out dic))
			{
				IScheduleRange range;
				if (dic.TryGetValue(person, out range))
					return range.ScheduledDay(restrictionDate);
			}

			var theDic = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, new List<IPerson> { person });
			return theDic[person].ScheduledDay(restrictionDate);
		}

		public void SetThisTime(ILastChangedReadModel lastTime, string step)
		{
			_timeValues.AddOrUpdate(step, lastTime, (key, oldValue) => lastTime);
		}

		public void UpdateThisTime(string step, IBusinessUnit businessUnit)
		{
			ILastChangedReadModel lastTime;
			if (_timeValues.TryGetValue(step, out lastTime))
			{
				_jobParameters.Helper.Repository.UpdateLastChangedDate(businessUnit, step, lastTime.ThisTime);
			}
		}

		public bool PermissionsMustRun()
		{
			ILastChangedReadModel lastTime;
			if (_timeValues.TryGetValue("Permissions", out lastTime))
			{
				return !lastTime.LastTime.Equals(lastTime.ThisTime);
			}
			return true;
		}

		public IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetCollection
		{
			get
			{
				return _multiplicatorDefinitionSetCollection ??
						 (_multiplicatorDefinitionSetCollection = _jobParameters.Helper.Repository.LoadMultiplicatorDefinitionSet());
			}
		}

		public IList<TimeZonePeriod> PeriodToLoadBridgeTimeZone
		{
			get
			{
				if (_bridgeTimeZonePeriodList == null)
				{
					_bridgeTimeZonePeriodList = _jobParameters.Helper.Repository.GetBridgeTimeZoneLoadPeriod(_jobParameters.DefaultTimeZone);
				}

				return _bridgeTimeZonePeriodList;
			}
		}

		//This only to be used in Test to avoid a load of too much data
		public void SetLoadBridgeTimeZonePeriod(DateTimePeriod period, string timeZoneCode)
		{
			_bridgeTimeZonePeriodList = new List<TimeZonePeriod> 
				{ 
					 new TimeZonePeriod 
						  { 
								TimeZoneCode = timeZoneCode, 
								PeriodToLoad = period 
						  } 
				};
		}

		private class ScheduleCacheCollection
		{
			private readonly IDictionary<IScenario, ScheduleCacheItem> _scenarioCacheDictionary =
				 new Dictionary<IScenario, ScheduleCacheItem>();

			public ScheduleCacheItem this[IScenario scenario]
			{
				get
				{
					ScheduleCacheItem cacheItem;
					if (!_scenarioCacheDictionary.TryGetValue(scenario, out cacheItem))
					{
						cacheItem = new ScheduleCacheItem();
						_scenarioCacheDictionary.Add(scenario, cacheItem);
					}

					return cacheItem;
				}
			}
		}

		private class ScheduleCacheItem
		{
			private readonly IDictionary<DateTimePeriod, IScheduleDictionary> _periodData =
				 new Dictionary<DateTimePeriod, IScheduleDictionary>();

			internal void Add(DateTimePeriod period, IScheduleDictionary dictionary)
			{
				IScheduleDictionary currentDictionary;
				if (_periodData.TryGetValue(period, out currentDictionary))
					_periodData.Remove(period);

				_periodData.Add(period, dictionary);
			}

			internal IScheduleDictionary GetFromCacheIfAvailable(DateTimePeriod period)
			{
				foreach (var currentPeriod in _periodData)
				{
					if (currentPeriod.Key.Contains(period))
					{
						return currentPeriod.Value;
					}
				}

				return null;
			}
		}
	}
}