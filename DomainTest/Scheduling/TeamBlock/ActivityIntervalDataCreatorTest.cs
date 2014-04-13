using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class ActivityIntervalDataCreatorTest
	{
		private MockRepository _mocks;
		private IActivityIntervalDataCreator _target;
		private ICreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IDictionary<IActivity, IList<ISkillIntervalData>> _skillIntervalDatasPerActivity;
		private Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>> _skillIntervalDatasPerActivityAndDate;
		private List<ISkillIntervalData> _skillIntevalDataList;
		private Dictionary<DateOnly, IList<ISkillIntervalData>> _skillIntervalDataListPerDate;
		private IActivity _activity;
		private Dictionary<DateTime, ISkillIntervalData> _skillIntervalDataPerTimeSpan;
		private ISkillIntervalData _skillIntervalData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_createSkillIntervalDataPerDateAndActivity = _mocks.StrictMock<ICreateSkillIntervalDataPerDateAndActivity>();
			_dayIntervalDataCalculator = _mocks.StrictMock<IDayIntervalDataCalculator>();
			_target = new ActivityIntervalDataCreator(_createSkillIntervalDataPerDateAndActivity, _dayIntervalDataCalculator);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_skillIntervalDatasPerActivity = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			_skillIntervalData = new SkillIntervalData(new DateTimePeriod(), 77, 76, 1, null, null);
			_activity = new Activity("hej");
			_skillIntevalDataList = new List<ISkillIntervalData> {_skillIntervalData};
			_skillIntervalDatasPerActivity.Add(_activity, _skillIntevalDataList);
			_skillIntervalDatasPerActivityAndDate = new Dictionary<DateOnly, IDictionary<IActivity, IList<ISkillIntervalData>>>();
			_skillIntervalDatasPerActivityAndDate.Add(DateOnly.MinValue, _skillIntervalDatasPerActivity);
			_skillIntervalDataListPerDate = new Dictionary<DateOnly, IList<ISkillIntervalData>>();
			_skillIntervalDataListPerDate.Add(DateOnly.MinValue, _skillIntevalDataList);
			_skillIntervalDataPerTimeSpan = new Dictionary<DateTime, ISkillIntervalData>();
			_skillIntervalDataPerTimeSpan.Add(new DateTimePeriod().StartDateTime, _skillIntervalData);
		}

		[Test]
		public void ShouldCreate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_createSkillIntervalDataPerDateAndActivity.CreateFor(_teamBlockInfo, _schedulingResultStateHolder))
					  .Return(_skillIntervalDatasPerActivityAndDate);
				Expect.Call(_dayIntervalDataCalculator.Calculate(_skillIntervalDataListPerDate))
					  .Return(_skillIntervalDataPerTimeSpan);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateFor(_teamBlockInfo, DateOnly.MinValue, _schedulingResultStateHolder);
				Assert.IsTrue(result[_activity].ContainsKey(new DateTimePeriod().StartDateTime));
				Assert.AreEqual(_skillIntevalDataList[0], result[_activity][new DateTimePeriod().StartDateTime]);
			}
		}
	}
}