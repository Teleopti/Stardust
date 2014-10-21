using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class LoadSkillDaysInDefaultScenario : ILoadSkillDaysInDefaultScenario
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;

		public LoadSkillDaysInDefaultScenario(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		public IEnumerable<ISkillDay> FindRange(DateOnlyPeriod futurePeriod, ISkill skill)
		{
			return _skillDayRepository.FindRange(futurePeriod, skill, _currentScenario.Current());
		}
	}
}