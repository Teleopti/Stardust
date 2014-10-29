using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;

		public QuickForecaster(IQuickForecasterWorkload quickForecasterWorkload, IFetchAndFillSkillDays fetchAndFillSkillDays)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
		}

		public void Execute(ISkill skill, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);
			foreach (var workload in skill.WorkloadCollection)
			{
				_quickForecasterWorkload.Execute(workload, historicalPeriod, futurePeriod, skillDays);
			}
		}
	}
}