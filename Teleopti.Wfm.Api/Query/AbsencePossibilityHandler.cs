using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Staffing;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class AbsencePossibilityHandler : IQueryHandler<AbsencePossibilityByPersonIdDto, AbsencePossibilityDto>
	{
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;
		private readonly IAbsenceStaffingPossibilityCalculator _absenceStaffingPossibilityCalculator;
		private readonly IPersonRepository _personRepository;

		public AbsencePossibilityHandler(IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider,
			IAbsenceStaffingPossibilityCalculator absenceStaffingPossibilityCalculator,
			IPersonRepository personRepository)
		{
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_absenceStaffingPossibilityCalculator = absenceStaffingPossibilityCalculator;
			_personRepository = personRepository;
		}
		
		[UnitOfWork]
		public QueryResultDto<AbsencePossibilityDto> Handle(AbsencePossibilityByPersonIdDto query)
		{
			var person = _personRepository.Get(query.PersonId);
			if (person == null)
			{
				return new QueryResultDto<AbsencePossibilityDto>
				{
					Successful = false,
					Message = $"Person with Id {0} not found"
				};
			}

			var period = _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(person, new DateOnly(query.StartDate), true);
			if (!period.HasValue) return new QueryResultDto<AbsencePossibilityDto>();

			var possibilityModels = _absenceStaffingPossibilityCalculator.CalculateIntradayIntervalPossibilities(person, period.Value);
			var possibilities = createPeriodStaffingPossibilityViewModels(possibilityModels);

			return new QueryResultDto<AbsencePossibilityDto>
			{
				Successful = true,
				Result = possibilities.ToArray()
			};
		}

		private IEnumerable<AbsencePossibilityDto> createPeriodStaffingPossibilityViewModels(
			IEnumerable<CalculatedPossibilityModel> calculatedPossibilityModels)
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

					periodStaffingPossibilityViewModels.Add(new AbsencePossibilityDto
					{
						StartTime = startTime,
						EndTime = endTime,
						Possibility = calculatedPossibilityModel.IntervalPossibilies[keys[i]]
					});
				}
			}
			return periodStaffingPossibilityViewModels;
		}
	}
}