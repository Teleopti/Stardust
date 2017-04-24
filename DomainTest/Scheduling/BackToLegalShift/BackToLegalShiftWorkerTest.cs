using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.BackToLegalShift
{
	[TestFixture]
	public class BackToLegalShiftWorkerTest
	{
		private MockRepository _mocks;
		private IBackToLegalShiftWorker _target;
		private ITeamBlockClearer _teamBlockClearer;
		private ISafeRollbackAndResourceCalculation _safeRollBackAndResourceCalculation;
		private ITeamBlockSingleDayScheduler _teamBlockSingleDayScheduler;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingOptions _schedulingOptions;
		private ShiftProjectionCache _roleModelShift;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IBlockInfo _blockInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_safeRollBackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_teamBlockSingleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
			_target = new BackToLegalShiftWorker(_teamBlockClearer, _safeRollBackAndResourceCalculation, _teamBlockSingleDayScheduler, null);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_roleModelShift = _mocks.StrictMock<ShiftProjectionCache>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mocks.Stub<ISchedulingResultStateHolder>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 9, 22, 2014, 9, 22));
		}

		[Test]
		public void ShouldReturnTrueIfRescheduleWorks()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockSingleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions,
					new DateOnly(2014, 9, 22), _roleModelShift, _rollbackService, _resourceCalculateDelayer, null,
					null, null, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).IgnoreArguments().Return(true);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ReSchedule(_teamBlockInfo, _schedulingOptions, _roleModelShift, _rollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder));
			}
		}

		[Test]
		public void ShouldReturnFalseIfRescheduleFails()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockSingleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions,
					new DateOnly(2014, 9, 22), _roleModelShift, _rollbackService, _resourceCalculateDelayer, null,
					null, null, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).IgnoreArguments().Return(false);
				Expect.Call(() => _safeRollBackAndResourceCalculation.Execute(_rollbackService, _schedulingOptions));
				Expect.Call(() => _rollbackService.ClearModificationCollection());
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ReSchedule(_teamBlockInfo, _schedulingOptions, _roleModelShift, _rollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder));
			}
		}

	}
}