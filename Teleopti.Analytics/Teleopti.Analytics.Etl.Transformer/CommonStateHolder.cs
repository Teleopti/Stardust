using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Analytics.Etl.Transformer
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
        private IList<IScheduleDay> _schedulePartCollection;
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
	    private Dictionary<DateTimePeriod, IScheduleDictionary> _dictionaryCashe;
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
            get
            {
                if (_scenarioCollection == null)
                {
                    // Load scenarios
                    _scenarioCollection = _jobParameters.Helper.Repository.LoadScenario();
                }
                return _scenarioCollection;
            }
        }

        public IList<ISkill> SkillCollection
        {
            get
            {
                if (_skillCollection == null)
                {
                    // Load skills
                    _skillCollection = _jobParameters.Helper.Repository.LoadSkill(ActivityCollection);
                }
                return _skillCollection;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IEnumerable<IPerson> AllLoadedPersons
    	{
    		get { throw new NotImplementedException(); }
    	}

    	public IList<IActivity> ActivityCollection
        {
            get
            {
                if (_activityCollection == null)
                {
                    // Load activities
                    _activityCollection = _jobParameters.Helper.Repository.LoadActivity();
                }
                return _activityCollection;
            }
        }

        public IList<IAbsence> AbsenceCollection
        {
            get
            {
                if (_absenceCollection == null)
                {
                    // Load absences
                    _absenceCollection = _jobParameters.Helper.Repository.LoadAbsence();
                }
                return _absenceCollection;
            }
        }

        public IList<IShiftCategory> ShiftCategoryCollection
        {
            get
            {
                if (_shiftCategoryCollection == null)
                {
                    // Load shift categories
                    _shiftCategoryCollection = _jobParameters.Helper.Repository.LoadShiftCategory();
                }
                return _shiftCategoryCollection;
            }
        }

        public IList<IDayOffTemplate>  DayOffTemplateCollection
        {
            get
            {
                if (_dayOffTemplateCollection == null)
                {
                    // Load day off templates
                    _dayOffTemplateCollection = _jobParameters.Helper.Repository.LoadDayOff();
                }
                return _dayOffTemplateCollection;
            }
        }

        public IList<IApplicationFunction> ApplicationFunctionCollection
        {
            get
            {
                if (_applicationFunctionCollection == null)
                {
                    // Load application functions
                    _applicationFunctionCollection = _jobParameters.Helper.Repository.LoadApplicationFunction();
                }
                return _applicationFunctionCollection;
            }
        }

        public IList<IAvailableData> AvailableDataCollection
        {
            get
            {
                if (_availableDataCollection == null)
                {
                    // Load available data
                    _availableDataCollection = _jobParameters.Helper.Repository.LoadAvailableData();
                }
                return _availableDataCollection;
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
            get
            {
                if (_applicationRoleCollection == null)
                {
                    // Load application roles
                    _applicationRoleCollection = _jobParameters.Helper.Repository.LoadApplicationRole(this);
                }
                return _applicationRoleCollection;
            }
        }

        public IScenario DefaultScenario
        {
            get
            {
                if (_defaultScenario == null)
                {
                    _defaultScenario = (from s in ScenarioCollectionDeletedExcluded
                                        where s.DefaultScenario
                                        select s).First();
                }

                return _defaultScenario;
            }
        }

        public IList<TimeZoneInfo> TimeZoneCollection
        {
            get
            {
                if (_timeZones == null)
                {
                    _timeZones = (IList<TimeZoneInfo>)_jobParameters.Helper.Repository.LoadTimeZonesInUse();
                    //_timeZones = _jobParameters.Helper.Repository.LoadTimeZonesInUse();

                    // Ensure that the default time zone always exist
                    if (!_timeZones.Contains(_jobParameters.DefaultTimeZone))
                    {
                        _timeZones.Add(_jobParameters.DefaultTimeZone);
                    }

                    // Ensure that UTC time zone always exist
                    TimeZoneInfo utc = TimeZoneInfo.FindSystemTimeZoneById("UTC");
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
            get
            {
                if (_personCollection == null)
                {
                    // Load persons
                    _personCollection =
                        _jobParameters.Helper.Repository.LoadPerson(this);
                }

                return _personCollection;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public IList<IPerson> PersonsWithIds(List<Guid> ids)
		{
			return PersonCollection.Where(person => ids.Contains(person.Id.GetValueOrDefault())).ToList();
		}

	    public IEnumerable<IContract> ContractCollection
        {
            get
            {
                if (_contractCollection == null)
                {
                    // Loads contracts
                    _contractCollection = _jobParameters.Helper.Repository.LoadContract();
                }
                return _contractCollection;
            }
        }

        public IEnumerable<IContractSchedule> ContractScheduleCollection
        {
            get
            {
                if (_contractScheduleCollection == null)
                {
                    _contractScheduleCollection = _jobParameters.Helper.Repository.LoadContractSchedule();
                }
                return _contractScheduleCollection;
            }
        }

        public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
        {
            get
            {
                if (_partTimePercentageCollection == null)
                {
                    _partTimePercentageCollection = _jobParameters.Helper.Repository.LoadPartTimePercentage();
                }
                return _partTimePercentageCollection;
            }
        }

        public IEnumerable<IRuleSetBag> RuleSetBagCollection
        {
            get
            {
                if (_ruleSetBagCollection == null)
                {
                    _ruleSetBagCollection = _jobParameters.Helper.Repository.LoadRuleSetBag();
                }
                return _ruleSetBagCollection;
            }
        }

        public IEnumerable<IGroupPage> UserDefinedGroupings
        {
            get
            {
                if (_userDefinedGroupings == null)
                {
                    _userDefinedGroupings = _jobParameters.Helper.Repository.LoadUserDefinedGroupings();
                }
                return _userDefinedGroupings;
            }
        }

        public IEnumerable<IBusinessUnit> BusinessUnitCollection
        {
            get { yield return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get { return new DateOnlyPeriod(DateOnly.Today,DateOnly.Today); }
        }

        public IScheduleDictionary GetSchedules(DateTimePeriod period, IScenario scenario)
        {
            var scheduleDictionary = _scheduleCache[scenario].GetFromCacheIfAvailable(period);
            if (scheduleDictionary==null)
            {
                scheduleDictionary = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, this);
                _scheduleCache[scenario].Add(period,scheduleDictionary);
            }
            return scheduleDictionary;
        }

		public IDictionary<DateTimePeriod, IScheduleDictionary> GetSchedules(IList<IScheduleChangedReadModel> changed, IScenario scenario)
		{
			_dictionaryCashe = new Dictionary<DateTimePeriod, IScheduleDictionary>();

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
				_dictionaryCashe.Add(period, scheduleDictionary);
			}
				
			return _dictionaryCashe;
		}

		public IDictionary<DateOnly, IScheduleDictionary> GetSchedules(HashSet<IStudentAvailabilityDay> days, IScenario scenario)
		{
			var dictionary = new Dictionary<DateOnly, IScheduleDictionary>();

			var groupedDays = days.GroupBy(c => c.RestrictionDate);
			foreach (var availabilityDay in groupedDays)
			{
				var theDate = availabilityDay.Key;
				// detta måste fixas med tidszon, eller???
				var utcDate = new DateTime(theDate.Date.Ticks, DateTimeKind.Utc);
				var period = new DateTimePeriod(utcDate, utcDate.AddDays(1).AddMilliseconds(-1));
				var personsIds = availabilityDay.Select(d => d.Person.Id.GetValueOrDefault()).ToList();
				var persons = PersonsWithIds(personsIds);
				var scheduleDictionary = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, persons);
				dictionary.Add(theDate, scheduleDictionary);
			}

			return dictionary;
		}

		public IDictionary<DateTimePeriod, IScheduleDictionary> GetScheduleCashe()
		{
			if(_dictionaryCashe == null)
				_dictionaryCashe = new Dictionary<DateTimePeriod, IScheduleDictionary>();
			return _dictionaryCashe;
		}

        public IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScenario scenario)
        {
            return _jobParameters.Helper.Repository.LoadSchedulePartsPerPersonAndDate(period, GetSchedules(period,scenario));
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

        public IDictionary<ISkill, IList<ISkillDay>> GetSkillDaysDictionary(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
        {
            return _jobParameters.Helper.Repository.LoadSkillDays(period, skills, scenario);
        }

        public IList<IPerson> UserCollection
        {
            get
            {
                if (_userCollection == null)
                {
                    // Load users
                    _userCollection =
                        _jobParameters.Helper.Repository.LoadUser();
                }

                return _userCollection;
            }
        }

        public IList<IScheduleDay> GetSchedulePartPerPersonAndDate(IScheduleDictionary scheduleDictionary)
        {
            // Extract one schedulepart per each person and date
            //if (_schedulePartCollection == null)
            //{
            _schedulePartCollection = new List<IScheduleDay>();
            DateTimePeriod period = scheduleDictionary.Period.VisiblePeriod;

            // Extract one schedulepart per each person and date
            foreach (IPerson person in scheduleDictionary.Keys)
            {
                foreach (DateTime dateTime in period.DateCollection())
                {
                    var dateOnly = new DateOnly(dateTime);
                    IScheduleDay schedulePart = scheduleDictionary[person].ScheduledDay(dateOnly);
                    if (schedulePart != null)
                    {
                        _schedulePartCollection.Add(schedulePart);
                    }
                }
            }
            //}

            return _schedulePartCollection;
        }

		public IList<IScheduleDay> GetSchedulePartPerPersonAndDate(
			IDictionary<DateTimePeriod, IScheduleDictionary> dictionary)
		{
			var ret = new List<IScheduleDay>();
			foreach (var key in dictionary.Keys)
			{
				ret.AddRange(GetSchedulePartPerPersonAndDate(dictionary[key]));
			}
			return ret;
		}

		public IScheduleDay GetSchedulePartOnPersonAndDate(IPerson person, DateOnly restrictionDate, IScenario scenario)
		{
			var period = new DateTimePeriod(new DateTime(restrictionDate.Date.Ticks, DateTimeKind.Utc), new DateTime(restrictionDate.Date.Ticks, DateTimeKind.Utc).AddDays(1).AddMilliseconds(-1));
			if (GetScheduleCashe().ContainsKey(period))
			{
				var dic = GetScheduleCashe()[period];
				if (dic.ContainsKey(person))
					return dic[person].ScheduledDay(restrictionDate);
			}

			var theDic = _jobParameters.Helper.Repository.LoadSchedule(period, scenario, new List<IPerson> {person});
			return theDic[person].ScheduledDay(restrictionDate);
		}

		private readonly ConcurrentDictionary<string, ILastChangedReadModel> _timeValues = new ConcurrentDictionary<string, ILastChangedReadModel>();
	    public void SetThisTime(ILastChangedReadModel lastTime, string step)
	    {
			_timeValues.AddOrUpdate(step, lastTime, (key, oldValue) => lastTime);
	    }

	    public void UpdateThisTime(string step, IBusinessUnit businessUnit)
	    {
		    ILastChangedReadModel lastTime;
			if (_timeValues.TryGetValue(step, out lastTime))
		    {
				_jobParameters.Helper.Repository.UpdateLastChangedDate(businessUnit,step,lastTime.ThisTime);
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
    		get {
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

                _periodData.Add(period,dictionary);
            }

            internal IScheduleDictionary GetFromCacheIfAvailable(DateTimePeriod period)
            {
                foreach (DateTimePeriod currentPeriod in _periodData.Keys)
                {
                    if (currentPeriod.Contains(period))
                    {
                        return _periodData[currentPeriod];
                    }
                }

                return null;
            }
		}
    }
}
