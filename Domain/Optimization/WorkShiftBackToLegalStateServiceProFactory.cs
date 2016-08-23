using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class WorkShiftBackToLegalStateServiceProFactory
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly SchedulingStateHolderAllSkillExtractor _allSkillExtractor;
		private readonly IWorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;
		private readonly IDeleteSchedulePartService _deleteService;

		public WorkShiftBackToLegalStateServiceProFactory(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			SchedulingStateHolderAllSkillExtractor allSkillExtractor,
			IWorkShiftLegalStateDayIndexCalculator dayIndexCalculator,
			IDeleteSchedulePartService deleteService)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_allSkillExtractor = allSkillExtractor;
			_dayIndexCalculator = dayIndexCalculator;
			_deleteService = deleteService;
		}

		public IWorkShiftBackToLegalStateServicePro Create()
		{
			var bitArrayCreator = new WorkShiftBackToLegalStateBitArrayCreator();
			// when we move the period to the method we can have all this in autofac
			var dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(_dailySkillForecastAndScheduledValueCalculator, _allSkillExtractor);
			var decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, _dayIndexCalculator);
			var workShiftBackToLegalStateStep = new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, _deleteService);
			return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, _workShiftMinMaxCalculator);
		}
	}
}