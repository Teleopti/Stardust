using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingLab
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly INow _now;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;

		public ForecastedStaffingLab(
			ISkillDayRepository skillDayRepository, 
			INow now, 
			ISkillRepository skillRepository, 
			IScenarioRepository scenarioRepository)
		{
			_skillDayRepository = skillDayRepository;
			_now = now;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void DoIt(Guid id)
		{
			var date = new DateOnly(_now.UtcDateTime());
			var skill = _skillRepository.Get(id);
			var scenario = _scenarioRepository.Get(id);
			var skillDay = _skillDayRepository.FindRange(new DateOnlyPeriod(date, date), skill, scenario).First();

			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				Console.WriteLine(skillStaffPeriod.Period.StartDateTime.ToShortTimeString() + " | " + skillStaffPeriod.FStaff);
			}
		}
	}
}