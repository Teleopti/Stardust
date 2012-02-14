using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public class SingleAgentRestrictionModel
    {
        private ReadOnlyCollection<IPerson> _loadedPersons;
        private DateOnlyPeriod _loadedDateOnlyPeriod;
        private readonly DateTimePeriod _loadedDateTimePeriod;
        private readonly ICccTimeZoneInfo _timeZoneInfo;
        private readonly IRuleSetProjectionService _ruleSetProjectionService;
        private ISchedulingResultStateHolder _stateHolder;
        private readonly IList<KeyValuePair<IPerson, DateOnly>> _personsAffectedPeriodDates = new List<KeyValuePair<IPerson, DateOnly>>();
    	readonly IDictionary<KeyValuePair<IPerson, DateOnly>, AgentInfoHelper> _personsAffectedPeriodDatesDic = new Dictionary<KeyValuePair<IPerson, DateOnly>, AgentInfoHelper>();
        private ISchedulingOptions _schedulingOptions;
        private KeyValuePair<IPerson, DateOnly> _selectedPersonDate;

        public SingleAgentRestrictionModel(DateTimePeriod dateTimePeriod, ICccTimeZoneInfo timeZoneInfo, IRuleSetProjectionService ruleSetProjectionService)
        {
            _loadedDateTimePeriod = dateTimePeriod;
            _timeZoneInfo = timeZoneInfo;
            _ruleSetProjectionService = ruleSetProjectionService;
            _loadedDateOnlyPeriod = _loadedDateTimePeriod.ToDateOnlyPeriod(_timeZoneInfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IList<KeyValuePair<IPerson, DateOnly>> PersonsAffectedPeriodDates
        {
            get { return _personsAffectedPeriodDates; }
        }

        public ISchedulingResultStateHolder StateHolder
        {
            get { return _stateHolder; }
        }

        public KeyValuePair<IPerson, DateOnly> SelectedPersonDate
        {
            get { return _selectedPersonDate; }
        }

        public void Initialize(IList<IPerson> persons, ISchedulingResultStateHolder stateHolder, ISchedulingOptions schedulingOptions)
        {
            _stateHolder = stateHolder;
            _loadedPersons = new ReadOnlyCollection<IPerson>(persons);
            _schedulingOptions = schedulingOptions;
            _personsAffectedPeriodDatesDic.Clear();
            PersonsAffectedPeriodDates.Clear();
            FindPersonsAffectedPeriodDates(_loadedPersons);
        }

        private void FindPersonsAffectedPeriodDates(IEnumerable<IPerson> peopleToLoad)
        {
            foreach (IPerson person in peopleToLoad)
            {
                IEnumerable<DateOnlyPeriod?> realPeriods = GetRealPeriods(person);
                SetDataForPerson(person, realPeriods);
            }

            Reload(peopleToLoad);
        }

        public void Reload(IEnumerable<IPerson> peopleToReload)
        {
            ReloadDataForPeople(peopleToReload);
        }

        //private delegate void InitAgentInfoDelegate(ISchedulingOptions schedulingOptions);

        //private void ReloadDataForPeople(IEnumerable<IPerson> peopleToReload)
        //{
        //    lock (peopleToReload)
        //    {
        //        IDictionary<InitAgentInfoDelegate, IAsyncResult> runnableList = new Dictionary<InitAgentInfoDelegate, IAsyncResult>();
        //        foreach (var person in peopleToReload)
        //        {
        //            foreach (var key in _personsAffectedPeriodDatesDic.Keys)
        //            {
        //                if (person.Equals(key.Key))
        //                {
        //                    var agentInfo = _personsAffectedPeriodDatesDic[key];
        //                    InitAgentInfoDelegate toRun = agentInfo.SchedulePeriodData;
        //                    IAsyncResult result = toRun.BeginInvoke(_schedulingOptions, null, null);
        //                    runnableList.Add(toRun, result);
        //                    agentInfo.SchedulePeriodData(_schedulingOptions);
        //                }
        //            }
        //        }

        //        //Sync all threads
        //        try
        //        {
        //            foreach (KeyValuePair<InitAgentInfoDelegate, IAsyncResult> thread in runnableList)
        //            {
        //                thread.Key.EndInvoke(thread.Value);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Trace.WriteLine(e.Message);
        //            throw;
        //        }
        //    }
        //}

        private void ReloadDataForPeople(IEnumerable<IPerson> peopleToReload)
        {
            foreach (var person in peopleToReload)
            {
                foreach (var key in _personsAffectedPeriodDatesDic.Keys)
                {
                    if (person.Equals(key.Key))
                    {
                        var agentInfo = _personsAffectedPeriodDatesDic[key];
                        agentInfo.SchedulePeriodData(_schedulingOptions);
                    }
                }
            }
        }

        private void SetDataForPerson(IPerson person, IEnumerable<DateOnlyPeriod?> realPeriods)
        {
            foreach (DateOnlyPeriod? onlyPeriod in realPeriods)
            {
                DateOnly startDate = onlyPeriod.Value.StartDate;
                if (_personsAffectedPeriodDatesDic.ContainsKey(new KeyValuePair<IPerson, DateOnly>(person, startDate)))
                    continue;
                var agentInfo = new AgentInfoHelper(person, startDate, StateHolder, _schedulingOptions, _ruleSetProjectionService);
                //agentInfo.SchedulePeriodData(_schedulingOptions);
                _personsAffectedPeriodDatesDic.Add(new KeyValuePair<IPerson, DateOnly>(person, startDate), agentInfo);
                PersonsAffectedPeriodDates.Add(new KeyValuePair<IPerson, DateOnly>(person, startDate));
            }
        }

        private IEnumerable<DateOnlyPeriod?> GetRealPeriods(IPerson person)
        {
            IList<DateOnlyPeriod?> realPeriods = new List<DateOnlyPeriod?>();
            foreach (DateOnly dateOnly in _loadedDateOnlyPeriod.DayCollection())
            {
                IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
                if (virtualSchedulePeriod.DateOnlyPeriod.StartDate == DateTime.MinValue) continue;
                if (!virtualSchedulePeriod.IsValid)
                    continue;

                if (!realPeriods.Contains(virtualSchedulePeriod.DateOnlyPeriod))
                    realPeriods.Add(virtualSchedulePeriod.DateOnlyPeriod);
            }
            return realPeriods;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
        public AgentInfoHelper GetRowData(int rowIndex)
        {
			if (PersonsAffectedPeriodDates.Count == 0)
				return null;
            if (rowIndex > PersonsAffectedPeriodDates.Count)
                return null;
            KeyValuePair<IPerson, DateOnly>  personDate = PersonsAffectedPeriodDates[rowIndex];

            return _personsAffectedPeriodDatesDic[personDate];
        }

        public int IndexOf()
        {
            return PersonsAffectedPeriodDates.IndexOf(SelectedPersonDate);
        }

        public void SetSelectedPersonDate(int rowIndex)
        {
            if (rowIndex > PersonsAffectedPeriodDates.Count)
                return;
            _selectedPersonDate = PersonsAffectedPeriodDates[rowIndex];
        }

        public int TotalNumberOfDaysOff(int rowIndex)
        {
            KeyValuePair<IPerson, DateOnly> currentPerson = PersonsAffectedPeriodDates[rowIndex];
            return _personsAffectedPeriodDatesDic[currentPerson].NumberOfDatesWithPreferenceOrScheduledDaysOff;
        }

        public void SortOnColumn(int colIndex, bool ascending)
        {
            IList<AgentInfoHelper> helpers = new List<AgentInfoHelper>();
            IList<AgentInfoHelper> sorted = new List<AgentInfoHelper>();
            foreach (AgentInfoHelper agentInfoHelper in _personsAffectedPeriodDatesDic.Values)
            {
                helpers.Add(agentInfoHelper);
            }
            switch (colIndex)
            {
                case 0:
                    sorted = Sort(new Collection<AgentInfoHelper>(helpers), "PersonName", ascending);
                    break;
                case 1:
                    sorted = SortByInt(new Collection<AgentInfoHelper>(helpers), "NumberOfWarnings", ascending);
                    break;
                case 2:
                    sorted = Sort(new Collection<AgentInfoHelper>(helpers), "PeriodType", ascending);
                    break;
                case 3:
                    sorted = SortByDateOnly(new Collection<AgentInfoHelper>(helpers), "StartDate", ascending);
                    break;
                case 4:
                    sorted = SortByDateOnly(new Collection<AgentInfoHelper>(helpers), "EndDate", ascending);
                    break;
                case 5:
                    sorted = SortByTimeSpan(new Collection<AgentInfoHelper>(helpers), "SchedulePeriodTargetTime", ascending);
                    break;
                case 6:
                    sorted = SortByInt(new Collection<AgentInfoHelper>(helpers), "SchedulePeriodTargetDaysOff", ascending);
                    break;
                case 7:
                    sorted = SortByTimeSpan(new Collection<AgentInfoHelper>(helpers), "CurrentContractTime", ascending);
                    break;
                case 8:
                    sorted = SortByInt(new Collection<AgentInfoHelper>(helpers), "CurrentDaysOff", ascending);
                    break;
                case 9:
                    sorted = SortByTimeSpan(new Collection<AgentInfoHelper>(helpers), "MinPossiblePeriodTime", ascending);
                    break;
                case 10:
                    sorted = SortByTimeSpan(new Collection<AgentInfoHelper>(helpers), "MaxPossiblePeriodTime", ascending);
                    break;
                case 11:
                    sorted = SortByInt(new Collection<AgentInfoHelper>(helpers), "NumberOfDatesWithPreferenceOrScheduledDaysOff", ascending);
                    break;
                case 12:
                    sorted = SortByInt(new Collection<AgentInfoHelper>(helpers), "NumberOfWarnings", ascending);
                    break;
            }
            
            ReSortPersonsAffectedDates(sorted);
        }

        private void ReSortPersonsAffectedDates(IEnumerable<AgentInfoHelper> sorted)
        {
            IDictionary<AgentInfoHelper, KeyValuePair<IPerson, DateOnly>> dictionary = new Dictionary<AgentInfoHelper, KeyValuePair<IPerson, DateOnly>>();

            foreach (KeyValuePair<KeyValuePair<IPerson, DateOnly>, AgentInfoHelper> pair in _personsAffectedPeriodDatesDic)
            {
                dictionary.Add(pair.Value, pair.Key);
            }
            PersonsAffectedPeriodDates.Clear();
            foreach (AgentInfoHelper helper in sorted)
            {
                KeyValuePair<IPerson, DateOnly> sortedPair = dictionary[helper];
                PersonsAffectedPeriodDates.Add(sortedPair);
            }
        }

        public static IList<T> Sort<T>(Collection<T> dataList, string sortingColumn, bool isAscending)
        {
            // Creates the parameter for the linq expression
            var param = Expression.Parameter(typeof(T), "dataItem");
            // Creates teh linq expression requried for the sorting
            var mySortExpression =
                Expression.Lambda<Func<T, object>>(Expression.Property(param, sortingColumn), param);

            // Holds the results of the sorting process
            List<T> result;
            // Gets a iquaryale list out of the data list
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                // Sorts the quaryable list in ascending order
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                // Sorts the quaryable list in discending order
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }

            // Returns the sorted list as a collection
            return result;
        }

        public static IList<T> SortByInt<T>(Collection<T> dataList, string sortingColumn, bool isAscending)
        {
            var param = Expression.Parameter(typeof(T), "dataItem");

            var mySortExpression =
                Expression.Lambda<Func<T, int>>(Expression.Property(param, sortingColumn), param);

            List<T> result;
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }

            return result;
        }

        public static IList<T> SortByDateOnly<T>(Collection<T> dataList, string sortingColumn, bool isAscending)
        {
            var param = Expression.Parameter(typeof(T), "dataItem");

            var mySortExpression =
                Expression.Lambda<Func<T, DateOnly>>(Expression.Property(param, sortingColumn), param);

            List<T> result;
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }

            return result;
        }

        public static IList<T> SortByTimeSpan<T>(Collection<T> dataList, string sortingColumn, bool isAscending)
        {
            var param = Expression.Parameter(typeof(T), "dataItem");

            var mySortExpression =
                Expression.Lambda<Func<T, TimeSpan>>(Expression.Property(param, sortingColumn), param);

            List<T> result;
            IQueryable<T> queryableList = dataList.AsQueryable();

            if (isAscending)
            {
                result = queryableList.OrderBy(mySortExpression.Compile()).ToList();
            }
            else
            {
                result = queryableList.OrderByDescending(mySortExpression.Compile()).ToList();
            }

            return result;
        }
    }
}