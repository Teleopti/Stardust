using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	/*
	*  PLEASE DONT ADD MORE STATE TO THIS TYPE!
	*/
	public class SchedulerStateHolder : ISchedulerStateHolder, IClearReferredShiftTradeRequests
	{
		private readonly ICollection<DateOnly> _daysToResourceCalculate = new HashSet<DateOnly>();
		private Lazy<IDictionary<Guid, IPerson>> _combinedFilteredAgents;
		private IDictionary<Guid, IPerson> _filteredPersonsOvertimeAvailability;
		private IDictionary<Guid, IPerson> _filteredPersonsHourlyAvailability;
		private bool _filterOnOvertimeAvailability;
		private bool _filterOnHourlyAvailability;

		public SchedulerStateHolder(IScenario loadScenario, IDateOnlyPeriodAsDateTimePeriod loadPeriod, IEnumerable<IPerson> allPermittedPersons, IDisableDeletedFilter disableDeleteFilter, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			RequestedScenario = loadScenario;
			CommonStateHolder = new CommonStateHolder(disableDeleteFilter);
			RequestedPeriod = loadPeriod;
			SchedulingResultState = schedulingResultStateHolder;
			ChoosenAgents = new List<IPerson>(allPermittedPersons);
			ResetFilteredPersons();
			ResetFilteredPersonsOvertimeAvailability();
		}

		public SchedulerStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, IDisableDeletedFilter disableDeletedFilter)
		{
			CommonStateHolder = new CommonStateHolder(disableDeletedFilter);
			SchedulingResultState = schedulingResultStateHolder;
			ChoosenAgents = new List<IPerson>();
			ResetFilteredPersonsOvertimeAvailability();
		}
		
		public ShiftTradeRequestStatusCheckerWithSchedule ShiftTradeRequestStatusChecker { get; set; }

		public void SetRequestedScenario(IScenario scenario)
		{
			RequestedScenario = scenario;
		}

		public IList<IPerson> ChoosenAgents { get; }

		public bool ConsiderShortBreaks { get; set; } = true;

		public ISchedulingResultStateHolder SchedulingResultState { get; }

		public IDictionary<Guid, IPerson> FilteredCombinedAgentsDictionary => _combinedFilteredAgents.Value;

		public IDictionary<Guid, IPerson> FilteredAgentsDictionary { get; private set; }

		private IDictionary<Guid, IPerson> combinedFilters()
		{
			var filter = FilteredAgentsDictionary;

			if (!_filterOnOvertimeAvailability && !_filterOnHourlyAvailability) return filter;

			if (_filterOnOvertimeAvailability)
				filter = filter.Intersect(_filteredPersonsOvertimeAvailability).ToDictionary(t => t.Key, t => t.Value);

			if(_filterOnHourlyAvailability)
				filter = filter.Intersect(_filteredPersonsHourlyAvailability).ToDictionary(t => t.Key, t => t.Value);

			return filter;
		}

		public IScheduleDictionary Schedules => SchedulingResultState.Schedules;

		public CommonStateHolder CommonStateHolder { get; }

		public IEnumerable<DateOnly> DaysToRecalculate
		{
			get
			{
				return _daysToResourceCalculate.OrderBy(d => d.Date).ToArray();
			}
		}

		public IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }


		public IScenario RequestedScenario { get; private set; }

		public void ClearDaysToRecalculate()
		{
			_daysToResourceCalculate.Clear();
		}

		public void LoadSchedules(IFindSchedulesForPersons findSchedulesForPersons, IEnumerable<IPerson> personsInOrganization, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod period)
		{
			if (findSchedulesForPersons == null) throw new ArgumentNullException(nameof(findSchedulesForPersons));
			if (period == null) throw new ArgumentNullException(nameof(period));

			SchedulingResultState.Schedules =
				findSchedulesForPersons.FindSchedulesForPersons(RequestedScenario, personsInOrganization, scheduleDictionaryLoadOptions, period, ChoosenAgents, true);
		}
		
		public void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			CommonNameDescription = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
		}

		public void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			CommonStateHolder.LoadCommonStateHolder(repositoryFactory, unitOfWork);
		}

		public void MarkDateToBeRecalculated(DateOnly dateToRecalculate)
		{
			_daysToResourceCalculate.Add(dateToRecalculate);
		}

		public void ResetFilteredPersons()
		{
			var allPermittedPersonIds = ChoosenAgents.Select(x => x.Id.GetValueOrDefault()).ToArray();
			FilteredAgentsDictionary =
				SchedulingResultState.LoadedAgents.Where(p => Array.IndexOf(allPermittedPersonIds, p.Id.Value) >= 0)
					.OrderBy(CommonNameDescription.BuildFor)
					.ToDictionary(p => p.Id.Value);
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void ResetFilteredPersonsOvertimeAvailability()
		{
			_filteredPersonsOvertimeAvailability = new Dictionary<Guid, IPerson>();
			_filterOnOvertimeAvailability = false;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void ResetFilteredPersonsHourlyAvailability()
		{
			_filteredPersonsHourlyAvailability = new Dictionary<Guid, IPerson>();
			_filterOnHourlyAvailability = false;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersonsOvertimeAvailability(IEnumerable<IPerson> selectedPersons)
		{
			_filteredPersonsOvertimeAvailability = selectedPersons.OrderBy(CommonNameDescription.BuildFor).ToDictionary(p => p.Id.Value);
			_filterOnOvertimeAvailability = true;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersonsHourlyAvailability(IList<IPerson> selectedPersons)
		{
			_filteredPersonsHourlyAvailability = selectedPersons.OrderBy(CommonNameDescription.BuildFor).ToDictionary(p => p.Id.Value);
			_filterOnHourlyAvailability = true;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersons(HashSet<Guid> selectedGuids)
		{
			var selectedPersons = new Dictionary<Guid, IPerson>();
			foreach (var person in SchedulingResultState.LoadedAgents)
			{
				if (selectedGuids.Contains(person.Id.Value) && !selectedPersons.ContainsKey(person.Id.Value))
				{
					selectedPersons.Add(person.Id.Value, person);
				}
			}
			FilteredAgentsDictionary = selectedPersons.Values.OrderBy(CommonNameDescription.BuildFor).ToDictionary(p => p.Id.Value);
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public CommonNameDescriptionSetting CommonNameDescription { get; private set; } = new CommonNameDescriptionSetting();
		
		public void ClearReferredShiftTradeRequests()
		{
			ShiftTradeRequestStatusChecker?.ClearReferredShiftTradeRequests();
		}
	}
}
