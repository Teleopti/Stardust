using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.Security;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class IntradayOptimizer2CreatorTest
	{
		private IntradayOptimizer2Creator _target;
		private MockRepository _mocks;

		private IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixContainerList;
		private IList<IScheduleMatrixOriginalStateContainer> _workShiftContainerList;
		private IIntradayDecisionMaker _decisionMaker;
		private IScheduleService _scheduleService;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		private IScheduleMatrixOriginalStateContainer _matrixContainer1;
		private IScheduleMatrixOriginalStateContainer _matrixContainer2;
		private IScheduleMatrixOriginalStateContainer _workShiftContainer1;
		private IScheduleMatrixOriginalStateContainer _workShiftContainer2;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IPersonSkillProvider _personSkillProvider;
		private ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private ISkillIntervalDataDivider _skillIntervalDataDivider;
		private ISkillIntervalDataAggregator _skillIntervalDataAggregator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrixContainer1 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_matrixContainer2 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_workShiftContainer1 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_workShiftContainer2 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixContainerList = new List<IScheduleMatrixOriginalStateContainer> { _matrixContainer1, _matrixContainer2 };
			_workShiftContainerList = new List<IScheduleMatrixOriginalStateContainer> { _workShiftContainer1, _workShiftContainer2 };
			_decisionMaker = _mocks.StrictMock<IIntradayDecisionMaker>();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			IOptimizationPreferences optimizerPreferences = new OptimizationPreferences();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();

			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_personSkillProvider = new PersonSkillProvider();
			_skillStaffPeriodToSkillIntervalDataMapper = _mocks.StrictMock<ISkillStaffPeriodToSkillIntervalDataMapper>();
			_skillIntervalDataDivider = _mocks.StrictMock<ISkillIntervalDataDivider>();
			_skillIntervalDataAggregator = _mocks.StrictMock<ISkillIntervalDataAggregator>();

			_target = new IntradayOptimizer2Creator(_scheduleMatrixContainerList,
			                                        _workShiftContainerList,
			                                        _decisionMaker,
			                                        _scheduleService,
			                                        optimizerPreferences,
			                                        _rollbackService,
			                                        _schedulingResultStateHolder,
			                                        _personSkillProvider, _mocks.DynamicMock<ICurrentTeleoptiPrincipal>(),
													_skillStaffPeriodToSkillIntervalDataMapper,
												   _skillIntervalDataDivider,
												   _skillIntervalDataAggregator);
		}

		[Test]
		public void VerifyCreate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrixContainer1.ScheduleMatrix)
					.Return(_matrix1);
				Expect.Call(_matrixContainer2.ScheduleMatrix)
					.Return(_matrix2);
			}
			using (_mocks.Playback())
			{
				IList<IIntradayOptimizer2> optimizers = _target.Create();
				Assert.AreEqual(_scheduleMatrixContainerList.Count, optimizers.Count);
			}
		}
	}
}
