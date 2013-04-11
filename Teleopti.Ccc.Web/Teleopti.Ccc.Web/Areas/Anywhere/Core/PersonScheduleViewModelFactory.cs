using System;
using System.Dynamic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class DailyStaffingMetricsViewModelFactory : IDailyStaffingMetricsViewModelFactory
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;

		public DailyStaffingMetricsViewModelFactory(ISkillRepository skillRepository, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_skillRepository = skillRepository;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		public DailyStaffingMetricsViewModel CreateViewModel(Guid skillId, DateTime date)
		{
			var skill = _skillRepository.Load(skillId);
			var dateOnly = new DateOnly(date);
			var skillDays=_skillDayRepository.FindRange(new DateOnlyPeriod(dateOnly, dateOnly), skill, _currentScenario.Current());
			var sumOfForecastedHours = skillDays.First().ForecastedIncomingDemand;
			return new DailyStaffingMetricsViewModel
				{
					ForecastedHours = sumOfForecastedHours.TotalHours
				};
		}
	}

	public class DailyStaffingMetricsViewModel
	{
		public double ForecastedHours { get; set; }
	}

	public interface IDailyStaffingMetricsViewModelFactory
	{
		DailyStaffingMetricsViewModel CreateViewModel(Guid skillId, DateTime date);
	}

	public class PersonScheduleViewModelFactory : IPersonScheduleViewModelFactory
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonScheduleViewModelMapper _personScheduleViewModelMapper;

		public PersonScheduleViewModelFactory(IPersonRepository personRepository, IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IAbsenceRepository absenceRepository, IPersonScheduleViewModelMapper personScheduleViewModelMapper)
		{
			_personRepository = personRepository;
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_absenceRepository = absenceRepository;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
		}

		public PersonScheduleViewModel CreateViewModel(Guid personId, DateTime date)
		{
			var data = new PersonScheduleData
				{
					Person = _personRepository.Get(personId), 
					Date = date, 
					PersonScheduleDayReadModel = _personScheduleDayReadModelRepository.ForPerson(new DateOnly(date), personId),
					Absences = _absenceRepository.LoadAllSortByName()
				};
			if (data.PersonScheduleDayReadModel != null && data.PersonScheduleDayReadModel.Shift != null)
				data.Shift = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(data.PersonScheduleDayReadModel.Shift);
			return _personScheduleViewModelMapper.Map(data);
		}
	}
}