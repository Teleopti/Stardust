using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("merge with base class", Toggles.ResourcePlanner_NoWhiteSpotWhenTargetDayoffIsBroken_77941)]
	public class WorkShiftBackToLegalStateServiceProFactoryNew : WorkShiftBackToLegalStateServiceProFactory
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;

		public WorkShiftBackToLegalStateServiceProFactoryNew(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, SchedulingStateHolderAllSkillExtractor allSkillExtractor, WorkShiftLegalStateDayIndexCalculator dayIndexCalculator, IDeleteSchedulePartService deleteService) : base(workShiftMinMaxCalculator, dailySkillForecastAndScheduledValueCalculator, allSkillExtractor, dayIndexCalculator, deleteService)
		{
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		}

		protected override IWorkShiftBackToLegalStateServicePro CreateInstance(WorkShiftBackToLegalStateStep workShiftBackToLegalStateStep)
		{
			return new WorkShiftBackToLegalStateServicePro(workShiftBackToLegalStateStep, _workShiftMinMaxCalculator);
		}
	}
	
	public class WorkShiftBackToLegalStateServiceProFactory
	{
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly SchedulingStateHolderAllSkillExtractor _allSkillExtractor;
		private readonly WorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;
		private readonly IDeleteSchedulePartService _deleteService;

		public WorkShiftBackToLegalStateServiceProFactory(IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			SchedulingStateHolderAllSkillExtractor allSkillExtractor,
			WorkShiftLegalStateDayIndexCalculator dayIndexCalculator,
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
			return CreateInstance(workShiftBackToLegalStateStep);
		}

		protected virtual IWorkShiftBackToLegalStateServicePro CreateInstance(WorkShiftBackToLegalStateStep workShiftBackToLegalStateStep)
		{
			return new WorkShiftBackToLegalStateServiceProOLD(workShiftBackToLegalStateStep, _workShiftMinMaxCalculator);
		}
	}
}