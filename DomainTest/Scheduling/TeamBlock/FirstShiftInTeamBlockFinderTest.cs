using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class FirstShiftInTeamBlockFinderTest
	{
		private MockRepository _mocks;
		private IFirstShiftInTeamBlockFinder _target;
		private IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private ITeamBlockInfo _teamBlockInfo;
		private IPerson _person;
		private IScheduleDictionary _schedules;
		private ShiftProjectionCache _shiftProjectionCache;
		private IBlockInfo _blockInfo;
		private IList<IPerson> _groupMembers;
		private ITeamInfo _teamInfo;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;
		private IEditableShift _editableShift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
			_target = new FirstShiftInTeamBlockFinder(_shiftProjectionCacheManager);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_person = PersonFactory.CreatePerson();
			_schedules = _mocks.StrictMock<IScheduleDictionary>();
			_shiftProjectionCache = _mocks.StrictMock<ShiftProjectionCache>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014,9,3,2014,9,3));
			_groupMembers = new List<IPerson>{_person};
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_editableShift = new EditableShift(new ShiftCategory("hej"));
		}

		[Test]
		public void ShouldReturnIfFound()
		{
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2014,9,3), TimeZoneInfo.Utc);
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2014, 9, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift,dateOnlyAsDateTimePeriod)).IgnoreArguments().Return(_shiftProjectionCache);
			}

			using (_mocks.Playback())
			{
				var result = _target.FindFirst(_teamBlockInfo, _person, new DateOnly(2014, 9, 3), _schedules);
				Assert.AreSame(_shiftProjectionCache, result);
			}
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2014, 9, 3))).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
			}

			using (_mocks.Playback())
			{
				var result = _target.FindFirst(_teamBlockInfo, _person, new DateOnly(2014, 9, 3), _schedules);
				Assert.IsNull(result);
			}
		}
	}
}