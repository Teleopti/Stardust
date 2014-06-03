using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
	[TestFixture]
	public class SameOpenHoursInTeamBlockSpecificationTest
	{
		private ISameOpenHoursInTeamBlockSpecification _target;
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingResultStateHolder _scheduleResultStartHolder;
		private IOpenHourForDate _openHourForDate;
		private ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private BlockInfo _blockInfo;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleResultStartHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_openHourForDate = _mock.StrictMock<IOpenHourForDate>();
			_createSkillIntervalDataPerDateAndActivity = _mock.StrictMock<ICreateSkillIntervalDataPerDateAndActivity>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2013, 12, 17, 2013, 12, 18));
			_target = new SameOpenHoursInTeamBlockSpecification(_openHourForDate, _createSkillIntervalDataPerDateAndActivity,
				_scheduleResultStartHolder);

		}

		[Test]
		public void HasDifferentOpenHourForTeamBlockDays()
		{
			var activity = new Activity("hej");
			//this is the open hours for two days = different end time
			var dateTimePeriodDay1 = new DateTimePeriod(2013, 12, 17, 2013, 12, 18);
			var dateTimePeriodDay2 = new DateTimePeriod(2013, 12, 18, 2013, 12, 19).ChangeStartTime(TimeSpan.FromHours(1));
			ISkillIntervalData skillIntervalDataForDay1 = new SkillIntervalData(dateTimePeriodDay1, 0, 0, 0, null, null);
			ISkillIntervalData skillIntervalDataForDay2 = new SkillIntervalData(dateTimePeriodDay2, 0, 0, 0, null, null);

			var skillIntervalDataPerDateAndActivity =
				new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();

			var intervalDataForDate1 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate1.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay1});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 17), intervalDataForDate1);

			var intervalDataForDate2 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate2.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay2});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 18), intervalDataForDate2);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _scheduleResultStartHolder))
					.Return(skillIntervalDataPerDateAndActivity);
				//sample date
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				//compare with both dates
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 18), intervalDataForDate2))
					.Return(dateTimePeriodDay2.TimePeriodLocal());
			}
			Assert.IsFalse(_target.IsSatisfiedBy(_teamBlockInfo));
		}

		[Test]
		public void HasSameOpenHourForTeamBlockDays()
		{
			var activity = new Activity("hej");
			//this is the open hours for two days = same start and end time
			var dateTimePeriodDay1 = new DateTimePeriod(2013, 12, 17, 2013, 12, 18);
			var dateTimePeriodDay2 = new DateTimePeriod(2013, 12, 18, 2013, 12, 19);
			ISkillIntervalData skillIntervalDataForDay1 = new SkillIntervalData(dateTimePeriodDay1, 0, 0, 0, null, null);
			ISkillIntervalData skillIntervalDataForDay2 = new SkillIntervalData(dateTimePeriodDay2, 0, 0, 0, null, null);

			var skillIntervalDataPerDateAndActivity =
				new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();

			var intervalDataForDate1 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate1.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay1});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 17), intervalDataForDate1);

			var intervalDataForDate2 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate2.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay2});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 18), intervalDataForDate2);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _scheduleResultStartHolder))
					.Return(skillIntervalDataPerDateAndActivity);
				//sample date
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				//compare with both dates
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 18), intervalDataForDate2))
					.Return(dateTimePeriodDay2.TimePeriodLocal());
			}
			Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
		}

		[Test]
		public void ShouldRetrurnTrueIfDayOffIsEncountered()
		{
			var activity = new Activity("hej");
			//this is the open hours for two days = same start and end time
			var dateTimePeriodDay1 = new DateTimePeriod(2013, 12, 17, 2013, 12, 18);
			var dateTimePeriodDay2 = new DateTimePeriod(2013, 12, 18, 2013, 12, 19);
			ISkillIntervalData skillIntervalDataForDay1 = new SkillIntervalData(dateTimePeriodDay1, 0, 0, 0, null, null);
			ISkillIntervalData skillIntervalDataForDay2 = new SkillIntervalData(dateTimePeriodDay2, 0, 0, 0, null, null);

			var skillIntervalDataPerDateAndActivity =
				new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();

			var intervalDataForDate1 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate1.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay1});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 17), intervalDataForDate1);

			var intervalDataForDate2 = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			intervalDataForDate2.Add(activity, new List<ISkillIntervalData> {skillIntervalDataForDay2});
			skillIntervalDataPerDateAndActivity.Add(new DateOnly(2013, 12, 18), intervalDataForDate2);

			using (_mock.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _scheduleResultStartHolder))
					.Return(skillIntervalDataPerDateAndActivity);
				//sample date
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				//compare with both dates
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 17), intervalDataForDate1))
					.Return(dateTimePeriodDay1.TimePeriodLocal());
				Expect.Call(_openHourForDate.OpenHours(new DateOnly(2013, 12, 18), intervalDataForDate2))
					.Return(null);
			}
			Assert.IsTrue(_target.IsSatisfiedBy(_teamBlockInfo));
		}
	}
}
