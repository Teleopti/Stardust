using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IWorkShiftBackToLegalStateServiceFactory
	{
		IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro();
	}

	public class WorkShiftBackToLegalStateServiceFactory : IWorkShiftBackToLegalStateServiceFactory
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly SchedulingStateHolderAllSkillExtractor _schedulingStateHolderAllSkillExtractor;
		private readonly IWorkShiftLegalStateDayIndexCalculator _workShiftLegalStateDayIndexCalculator;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;

		public WorkShiftBackToLegalStateServiceFactory(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, SchedulingStateHolderAllSkillExtractor schedulingStateHolderAllSkillExtractor, IWorkShiftLegalStateDayIndexCalculator workShiftLegalStateDayIndexCalculator, IDeleteSchedulePartService deleteSchedulePartService)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_schedulingStateHolderAllSkillExtractor = schedulingStateHolderAllSkillExtractor;
			_workShiftLegalStateDayIndexCalculator = workShiftLegalStateDayIndexCalculator;
			_deleteSchedulePartService = deleteSchedulePartService;
		}

		public IWorkShiftBackToLegalStateServicePro CreateWorkShiftBackToLegalStateServicePro()
		{
			var bitArrayCreator = new WorkShiftBackToLegalStateBitArrayCreator();

			// when we move the period to the method and remove the other constructor we can have all this in autofac
			var dataExtractor = new RelativeDailyDifferencesByAllSkillsExtractor(_dailySkillForecastAndScheduledValueCalculator, _schedulingStateHolderAllSkillExtractor);

			var decisionMaker = new WorkShiftBackToLegalStateDecisionMaker(dataExtractor, _workShiftLegalStateDayIndexCalculator);
			var workShiftBackToLegalStateStep = new WorkShiftBackToLegalStateStep(bitArrayCreator, decisionMaker, _deleteSchedulePartService);
			return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, _workShiftMinMaxCalculator);
		} 
	}
}