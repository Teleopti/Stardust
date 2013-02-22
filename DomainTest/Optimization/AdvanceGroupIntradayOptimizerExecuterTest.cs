using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class AdvanceGroupIntradayOptimizerExecuterTest
	{
		private MockRepository _mock;
		private IAdvanceGroupIntradayOptimizerExecuter _target;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IDeleteAndResourceCalculateService _deleteService;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IOptimizationPreferences _optimizerPreferences;
		private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private IGroupMatrixHelper _groupMatrixHelper;
		private IGroupSchedulingService _groupSchedulingService;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IList<IScheduleDay> _daysToSave;
		private IList<IScheduleDay> _daysToDelete;
		private IList<IScheduleMatrixPro> _allMatrixes;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay;
		private ISchedulingOptions _schedulingOptions;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IMainShift _mainShift;
		private IPersonAssignment _personAssignment;
		private IPerson _person;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
		private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private IWorkShiftFilterService _workShiftFilterService;
		private IWorkShiftSelector _workShiftSelector;
		private ITeamScheduling _teamScheduling;
		private IRestrictionAggregator _restrictionAggregator;
		private IDynamicBlockFinder _dynamicBlockFinder;
		private IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_deleteService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_optimizerPreferences = new OptimizationPreferences();
			_mainShiftOptimizeActivitySpecificationSetter = _mock.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
			_groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
			_resourceOptimizationHelper = _mock.StrictMock<IResourceOptimizationHelper>();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_skillDayPeriodIntervalDataGenerator = _mock.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
			_workShiftFilterService = _mock.StrictMock<IWorkShiftFilterService>();
			_workShiftSelector = _mock.StrictMock<IWorkShiftSelector>();
			_teamScheduling = _mock.StrictMock<ITeamScheduling>();
			_restrictionAggregator = _mock.StrictMock<IRestrictionAggregator>();
			_dynamicBlockFinder = _mock.StrictMock<IDynamicBlockFinder>();
			_groupPersonBuilderBasedOnContractTime = _mock.StrictMock<IGroupPersonBuilderBasedOnContractTime>();
			_target = new AdvanceGroupIntradayOptimizerExecuter(_schedulePartModifyAndRollbackService, _deleteService,
			                                                    _schedulingOptionsCreator, _optimizerPreferences,
			                                                    _mainShiftOptimizeActivitySpecificationSetter,
			                                                    _groupPersonBuilderForOptimization,
			                                                    _resourceOptimizationHelper, _restrictionAggregator,
			                                                    _dynamicBlockFinder, _groupPersonBuilderBasedOnContractTime,
			                                                    _schedulingOptions, _skillDayPeriodIntervalDataGenerator,
			                                                    _workShiftFilterService, _workShiftSelector, _teamScheduling);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_daysToDelete = new List<IScheduleDay> { _scheduleDay };
			_daysToSave = new List<IScheduleDay> { _scheduleDay };
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_schedulingOptions = new SchedulingOptions();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_mainShift = _mock.StrictMock<IMainShift>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_person = new Person();

			_optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();

		}
	}
}