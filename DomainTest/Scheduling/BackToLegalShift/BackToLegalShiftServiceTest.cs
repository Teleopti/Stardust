using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.BackToLegalShift
{
	[TestFixture]
	public class BackToLegalShiftServiceTest
	{
		private MockRepository _mocks;
		private IBackToLegalShiftService _target;
		private IBackToLegalShiftWorker _backToLegalShiftWorker;
		private IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private ILegalShiftDecider _legalShiftDecider;
		private ITeamBlockInfo _teamBlock;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ISchedulePartModifyAndRollbackService _rollBackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ITeamInfo _teamInfo;
		private IPerson _person;
		private IBlockInfo _blockInfo;
		private IShiftProjectionCache _shiftProjectionCache;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_backToLegalShiftWorker = _mocks.StrictMock<IBackToLegalShiftWorker>();
			_firstShiftInTeamBlockFinder = _mocks.StrictMock<IFirstShiftInTeamBlockFinder>();
			_legalShiftDecider = _mocks.StrictMock<ILegalShiftDecider>();
			_target = new BackToLegalShiftService(_backToLegalShiftWorker, _firstShiftInTeamBlockFinder, _legalShiftDecider);
			_teamBlock = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_rollBackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly());
			_person.Period(new DateOnly()).RuleSetBag = new RuleSetBag();
			_blockInfo = new BlockInfo(new DateOnlyPeriod());
			_shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
		}

		[Test]
		public void ShouldRescheduleIfShiftNotLegal()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> {_person});
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, new DateOnly(), _schedulingResultStateHolder))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(new DateOnly(), _person.PermissionInformation.DefaultTimeZone(),
					_person.Period(new DateOnly()).RuleSetBag, _shiftProjectionCache)).Return(false);
				Expect.Call(_backToLegalShiftWorker.ReSchedule(_teamBlock, _schedulingOptions, _shiftProjectionCache,
					_rollBackService, _resourceCalculateDelayer, _schedulingResultStateHolder, true)).Return(true);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> {_teamBlock}, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer, true);
			}
		}

		[Test]
		public void ShouldNotRescheduleIfShiftLegal()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, new DateOnly(), _schedulingResultStateHolder))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(new DateOnly(), _person.PermissionInformation.DefaultTimeZone(),
					_person.Period(new DateOnly()).RuleSetBag, _shiftProjectionCache)).Return(true);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer, true);
			}
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldTrowIfTeamMembersMoreThanOne()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person, _person });
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer, true);
			}
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldTrowIfBlockPeriodMoreThanOneDay()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014,9,23,2014,9,24));
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer, true);
			}
		}

	}
}