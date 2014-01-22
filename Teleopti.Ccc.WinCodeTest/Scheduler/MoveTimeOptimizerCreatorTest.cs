using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class MoveTimeOptimizerCreatorTest
	{
		private MoveTimeOptimizerCreator _target;
		private MockRepository _mocks;

		private IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixOriginalStateContainerList;
		private IList<IScheduleMatrixOriginalStateContainer> _workShiftOriginalStateContainerList;
		private IMoveTimeDecisionMaker _decisionMaker;
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
			_scheduleMatrixOriginalStateContainerList = new List<IScheduleMatrixOriginalStateContainer> { _matrixContainer1, _matrixContainer2 };
			_workShiftOriginalStateContainerList = new List<IScheduleMatrixOriginalStateContainer> { _workShiftContainer1, _workShiftContainer2 };
			_decisionMaker = _mocks.StrictMock<IMoveTimeDecisionMaker>();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			IOptimizationPreferences optimizerPreferences = new OptimizationPreferences();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_personSkillProvider = new PersonSkillProvider();
			_target = new MoveTimeOptimizerCreator(_scheduleMatrixOriginalStateContainerList,
												   _workShiftOriginalStateContainerList,
												   _decisionMaker,
												   _scheduleService,
												   optimizerPreferences,
												   _rollbackService,
												   _schedulingResultStateHolder,
												   _personSkillProvider, 
												   new CurrentTeleoptiPrincipal(),
												   new ScheduleMatrixLockableBitArrayConverterEx());
		}

		[Test]
		public void VerifyCreateOneOptimizerForEachMatrix()
		{

			using (_mocks.Record())
			{
				Expect.Call(_matrixContainer1.ScheduleMatrix)
					.Return(_matrix1)
					.Repeat.AtLeastOnce();
				Expect.Call(_matrixContainer2.ScheduleMatrix)
					.Return(_matrix2)
					.Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules)
					.Return(null)
					.Repeat.Any();
				Expect.Call(_matrix1.SchedulingStateHolder)
					.Return(_schedulingResultStateHolder)
					.Repeat.Any();
				Expect.Call(_matrix2.SchedulingStateHolder)
					.Return(_schedulingResultStateHolder)
					.Repeat.Any();
			}
			using (_mocks.Playback())
			{
				IList<IMoveTimeOptimizer> optimizers = _target.Create();
				Assert.AreEqual(_scheduleMatrixOriginalStateContainerList.Count, optimizers.Count);
			}
		}
	}
}
