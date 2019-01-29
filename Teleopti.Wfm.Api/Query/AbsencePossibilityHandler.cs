using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class AbsencePossibilityHandler : IQueryHandler<AbsencePossibilityByPersonIdDto, AbsencePossibilityDto>
	{
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;
		private readonly IAbsenceStaffingPossibilityCalculator _absenceStaffingPossibilityCalculator;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public AbsencePossibilityHandler(IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			IAbsenceStaffingPossibilityCalculator absenceStaffingPossibilityCalculator,
			IPersonRepository personRepository, ICurrentAuthorization currentAuthorization,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository)
		{
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_absenceStaffingPossibilityCalculator = absenceStaffingPossibilityCalculator;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_currentAuthorization = currentAuthorization;
		}

		[UnitOfWork]
		public virtual QueryResultDto<AbsencePossibilityDto> Handle(AbsencePossibilityByPersonIdDto query)
		{
			var person = _personRepository.Get(query.PersonId);
			if (person == null)
			{
				return new QueryResultDto<AbsencePossibilityDto>
				{
					Successful = false,
					Message = $"Person with Id {query.PersonId} could not be found"
				};
			}

			var startDate = query.StartDate.ToDateOnly();
			if (!_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb,
				startDate, person))
			{
				return new QueryResultDto<AbsencePossibilityDto>
				{
					Successful = false,
					Message =
						$"Person with Id {query.PersonId} is not allowed to request absence in {query.StartDate:yyyy-MM-dd}"
				};
			}

			var periodForAbsence = _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(person, startDate, true);
			if (!periodForAbsence.HasValue)
			{
				return new QueryResultDto<AbsencePossibilityDto>
				{
					Successful = false,
					Message = $"There is no staffing data available period for given date {query.StartDate:yyyy-MM-dd}"
				};
			}

			var loadOption = new ScheduleDictionaryLoadOptions(false, false) {LoadDaysAfterLeft = false};
			var period = new DateOnlyPeriod(query.StartDate.ToDateOnly(), query.EndDate.ToDateOnly());
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var schedule =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, loadOption, period, scenario);
			var scheduledTimePeriods = schedule[person].ScheduledDayCollection(period)
				.SelectMany(x => x.ProjectionService().CreateProjection().FilterLayers<IActivity>()).ToArray();

			var possibilityModels = _absenceStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(person,
				periodForAbsence.Value);
			var possibilities = createPeriodStaffingPossibilityViewModels(possibilityModels, scheduledTimePeriods,
				person.PermissionInformation.DefaultTimeZone());

			return new QueryResultDto<AbsencePossibilityDto>
			{
				Successful = true,
				Result = possibilities.ToArray()
			};
		}

		private IEnumerable<AbsencePossibilityDto> createPeriodStaffingPossibilityViewModels(
			IEnumerable<CalculatedPossibilityModel> calculatedPossibilityModels,
			IVisualLayer[] scheduledTimePeriods, TimeZoneInfo timezone)
		{
			var periodStaffingPossibilityViewModels = new List<AbsencePossibilityDto>();
			foreach (var calculatedPossibilityModel in calculatedPossibilityModels)
			{
				var keys = calculatedPossibilityModel.IntervalPossibilies.Keys.OrderBy(x => x).ToArray();
				for (var i = 0; i < keys.Length; i++)
				{
					var startTime = keys[i];
					var endTime = i == keys.Length - 1
						? startTime.AddMinutes(calculatedPossibilityModel.Resolution)
						: keys[i + 1];

					var period = new DateTimePeriod(
						TimeZoneHelper.ConvertToUtc(startTime, timezone),
						TimeZoneHelper.ConvertToUtc(endTime, timezone));
					if (!scheduledTimePeriods.Any(p => p.WorkTime() > TimeSpan.Zero && p.Period.Contains(period)))
					{
						continue;
					}

					periodStaffingPossibilityViewModels.Add(new AbsencePossibilityDto
					{
						StartTime = startTime,
						EndTime = endTime,
						Possibility = calculatedPossibilityModel.IntervalPossibilies[startTime]
					});
				}
			}

			return periodStaffingPossibilityViewModels;
		}
	}
}
