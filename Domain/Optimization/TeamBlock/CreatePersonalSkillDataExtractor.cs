using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class CreatePersonalSkillDataExtractor
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly ForecastAndScheduleSumForDay _forecastAndScheduleSumForDay;

		public CreatePersonalSkillDataExtractor(PersonalSkillsProvider personalSkillsProvider, 
			ForecastAndScheduleSumForDay forecastAndScheduleSumForDay)
		{
			_personalSkillsProvider = personalSkillsProvider;
			_forecastAndScheduleSumForDay = forecastAndScheduleSumForDay;
		}
		
		public IScheduleResultDataExtractor Create(IScheduleMatrixPro scheduleMatrix, IOptimizationPreferences optimizationPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix, _personalSkillsProvider);
			return new scheduleResultDataExtractor(optimizationPreferences, scheduleMatrix, skillExtractor, schedulingResultStateHolder, _forecastAndScheduleSumForDay);
		}
		
		private class scheduleResultDataExtractor : IScheduleResultDataExtractor
		{
			private readonly IOptimizationPreferences _optimizationPreferences;
			private readonly IScheduleMatrixPro _scheduleMatrixPro;
			private readonly ISkillExtractor _personalSkillExtractor;
			private readonly ISchedulingResultStateHolder _stateHolder;
			private readonly ForecastAndScheduleSumForDay _forecastAndScheduleSumForDay;
	
	
			public scheduleResultDataExtractor(IOptimizationPreferences optimizationPreferences,
				IScheduleMatrixPro scheduleMatrixPro, 
				ISkillExtractor personalSkillExtractor,
				ISchedulingResultStateHolder stateHolder,
				ForecastAndScheduleSumForDay forecastAndScheduleSumForDay) 
			{
				_optimizationPreferences = optimizationPreferences;
				_scheduleMatrixPro = scheduleMatrixPro;
				_personalSkillExtractor = personalSkillExtractor;
				_stateHolder = stateHolder;
				_forecastAndScheduleSumForDay = forecastAndScheduleSumForDay;
			}
			
			public IList<double?> Values()
			{
				return _scheduleMatrixPro.EffectivePeriodDays
					.Select(scheduleDayPro => dayValue(_personalSkillExtractor.ExtractSkills(), scheduleDayPro.Day)).ToArray();
			}
			
			private double? dayValue(IEnumerable<ISkill> skills, DateOnly scheduleDay)
			{
				var sumValues = _forecastAndScheduleSumForDay.Execute(_optimizationPreferences, _stateHolder, skills, scheduleDay);
				
				return new DeviationStatisticData(sumValues.ForecastSum, sumValues.ScheduledSum).RelativeDeviation;
			}
		}
	}
}