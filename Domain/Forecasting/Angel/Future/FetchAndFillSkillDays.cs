using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class FetchAndFillSkillDays : IFetchAndFillSkillDays
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IFillWithEmptySkillDays _fillEmptySkillDays;

		public FetchAndFillSkillDays(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, IFillWithEmptySkillDays fillEmptySkillDays)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_fillEmptySkillDays = fillEmptySkillDays;
		}

		public ICollection<ISkillDay> FindRange(DateOnlyPeriod futurePeriod, ISkill skill)
		{
			var currentScenario = _currentScenario.Current();
			var skillDays = _skillDayRepository.FindRange(futurePeriod, skill, currentScenario);
			_fillEmptySkillDays.GetAllSkillDays(futurePeriod, skillDays, skill, currentScenario, _skillDayRepository.AddRange);
			return skillDays;
		}
	}
}