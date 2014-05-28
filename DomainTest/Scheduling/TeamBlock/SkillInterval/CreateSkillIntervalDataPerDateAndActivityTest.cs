using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.SkillInterval
{
	[TestFixture]
	public class CreateSkillIntervalDataPerDateAndActivityTest
	{
		private ICreateSkillIntervalDataPerDateAndActivity _target;
		private IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private MockRepository _mock;
		private ICreateSkillIntervalDatasPerActivtyForDate _createSkillIntervalDatasPerActivtyForDate;
		private IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private ISkillIntervalDataDivider _intervalDataDivider;
		private ITeamBlockInfo _teamBlockInfo;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ITeamInfo _teamInfo;
		private IPerson _person1;
		private IBlockInfo _blockInfo;
		private ISkill _skill1;
		private ISkill _maxSeatSkill;
		private IActivity _activity;
		private ISkillIntervalData _skillInterval1;
		private ISkillIntervalData _skillInterval2;
		private ISkillIntervalData _splittedSkillInterval1;
		private ISkillIntervalData _splittedSkillInterval2;
		private ISkillIntervalData _splittedSkillInterval3;
		private ISkillIntervalData _splittedSkillInterval4;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();

			_groupPersonSkillAggregator = _mock.StrictMock<IGroupPersonSkillAggregator>();
			_createSkillIntervalDatasPerActivtyForDate = _mock.StrictMock<ICreateSkillIntervalDatasPerActivtyForDate>();
			_maxSeatSkillAggregator = _mock.StrictMock<IMaxSeatSkillAggregator>();
			_intervalDataDivider = _mock.StrictMock<ISkillIntervalDataDivider>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();

			_skillInterval1 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 0, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 16, 30, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);
			_skillInterval2 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 17, 0, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);
			_splittedSkillInterval1 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 0, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 16, 15, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);
			_splittedSkillInterval2 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 15, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 16, 30, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);
			_splittedSkillInterval3 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 30, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 16, 45, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);
			_splittedSkillInterval4 = new SkillIntervalData(new DateTimePeriod(new DateTime(2014, 05, 28, 16, 45, 0, DateTimeKind.Utc), new DateTime(2014, 05, 28, 17, 0, 0, DateTimeKind.Utc)), 0, 0, 0, null, null);

			_activity = ActivityFactory.CreateActivity("activity");
			_person1 = PersonFactory.CreatePerson("test1");
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014,05,28,2014,05,28));
			_skill1 = SkillFactory.CreateSkill("skill1");
			_maxSeatSkill = SkillFactory.CreateSkill("maxSeatSkill1");
			_target = new CreateSkillIntervalDataPerDateAndActivity(_groupPersonSkillAggregator,
				_createSkillIntervalDatasPerActivtyForDate, _maxSeatSkillAggregator, _intervalDataDivider);
		}

		[Test]
		public void ShouldExecuteSuccessfully()
		{
			IEnumerable<IPerson> personList = new List<IPerson> {_person1};
			IEnumerable<ISkill> skillList = new List<ISkill> {_skill1};
			var maxSeatSkillList  =new HashSet<ISkill>{_maxSeatSkill};
			var activityToIntervalList = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			IList<ISkillIntervalData> skillIntervalList = new List<ISkillIntervalData>() {_skillInterval1, _skillInterval2};
			IList<ISkillIntervalData> spliedIntervalList = new List<ISkillIntervalData>() { _splittedSkillInterval1, _splittedSkillInterval2, _splittedSkillInterval3, _splittedSkillInterval4};
			activityToIntervalList.Add(_activity,skillIntervalList);
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(personList);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(personList, _blockInfo.BlockPeriod)).Return(skillList);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(personList.ToList(), _blockInfo.BlockPeriod))
					.Return(maxSeatSkillList);

				
				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);
				
				Expect.Call(_intervalDataDivider.SplitSkillIntervalData(skillIntervalList, 15)).Return(spliedIntervalList);
				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28).AddDays(1), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);
			}
			using (_mock.Playback() )
			{
				var result =  _target.CreateFor(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.AreEqual(2,result.Count );
			}
		}

		[Test]
		public void ShouldExecuteSuccessfullyIfNoMaxSeatSkill()
		{
			IEnumerable<IPerson> personList = new List<IPerson> { _person1 };
			IEnumerable<ISkill> skillList = new List<ISkill> { _skill1 };
			var activityToIntervalList = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			IList<ISkillIntervalData> skillIntervalList = new List<ISkillIntervalData>() { _skillInterval1, _skillInterval2 };
			activityToIntervalList.Add(_activity, skillIntervalList);
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(personList);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(personList, _blockInfo.BlockPeriod)).Return(skillList);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(personList.ToList(), _blockInfo.BlockPeriod))
					.Return(new HashSet<ISkill>());


				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);

				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28).AddDays(1), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);
			}
			using (_mock.Playback())
			{
				var result = _target.CreateFor(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.AreEqual(2, result.Count);
			}
		}

		[Test]
		public void ShouldExecuteSuccessfullyIfNoSkillIntervalDataFound()
		{
			IEnumerable<IPerson> personList = new List<IPerson> { _person1 };
			IEnumerable<ISkill> skillList = new List<ISkill> { _skill1 };
			var activityToIntervalList = new Dictionary<IActivity, IList<ISkillIntervalData>>();
			IList<ISkillIntervalData> skillIntervalList = new List<ISkillIntervalData>() ;
			activityToIntervalList.Add(_activity, skillIntervalList);
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(personList);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_groupPersonSkillAggregator.AggregatedSkills(personList, _blockInfo.BlockPeriod)).Return(skillList);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(personList.ToList(), _blockInfo.BlockPeriod))
					.Return(new HashSet<ISkill>());


				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);

				Expect.Call(_createSkillIntervalDatasPerActivtyForDate.CreateFor(new DateOnly(2014, 05, 28).AddDays(1), skillList.ToList(),
					_schedulingResultStateHolder)).Return(activityToIntervalList);
			}
			using (_mock.Playback())
			{
				var result = _target.CreateFor(_teamBlockInfo, _schedulingResultStateHolder);
				Assert.AreEqual(2, result.Count);
			}
		}
	}
}
