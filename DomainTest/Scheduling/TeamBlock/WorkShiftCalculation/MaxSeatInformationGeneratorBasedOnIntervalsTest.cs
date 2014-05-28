using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class MaxSeatInformationGeneratorBasedOnIntervalsTest
	{
		private MockRepository _mock;
		private IMaxSeatInformationGeneratorBasedOnIntervals _target;
		private IMaxSeatsSpecificationDictionaryExtractor _maxSeatsSpecificationDictionaryExtractor;
		private IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private DateOnlyPeriod _datePeriod;
		private ITeamBlockInfo  _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IEnumerable<IPerson> _personList;
		private HashSet<ISkill> _skillsHash;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IList<ISkillDay> _skillDays;
		private ISkillDay _skillDay1;
		private ReadOnlyCollection<ISkillStaffPeriod> _skillStaffPeriodCollection;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private IList<ISkillStaffPeriod> _skillStaffPeriodList;
		private ISkill _skill1;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_maxSeatsSpecificationDictionaryExtractor = _mock.StrictMock<IMaxSeatsSpecificationDictionaryExtractor>();
			_maxSeatSkillAggregator = _mock.StrictMock<IMaxSeatSkillAggregator>();
			_skillDay1 = _mock.StrictMock<ISkillDay>();
			_target = new MaxSeatInformationGeneratorBasedOnIntervals(_maxSeatSkillAggregator,
				_maxSeatsSpecificationDictionaryExtractor);
			_datePeriod = new DateOnlyPeriod(2014,05,28,2014,05,28);
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_personList = new List<IPerson> {PersonFactory.CreatePerson("test")};
			_skill1 = SkillFactory.CreateSkill("skill1");
			_skillsHash = new HashSet<ISkill>(){_skill1};
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_skillDays = new List<ISkillDay>(){_skillDay1};
			_skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodList = new List<ISkillStaffPeriod>{_skillStaffPeriod1,_skillStaffPeriod2};
			_skillStaffPeriodCollection = new ReadOnlyCollection<ISkillStaffPeriod>(_skillStaffPeriodList);
		}

		[Test]
		public void ShouldExecuteSuccessfully()
		{
			IDictionary<DateTime, bool> result = new Dictionary<DateTime, bool>();
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(_personList);
				Expect.Call(_maxSeatSkillAggregator.GetAggregatedSkills(_personList.ToList(), _datePeriod)).Return(_skillsHash);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {new DateOnly(2014, 05, 28)}))
					.Return(_skillDays);
				Expect.Call(_skillDay1.Skill).Return(_skill1);
				Expect.Call(_skillDays[0].SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection);
				Expect.Call(_maxSeatsSpecificationDictionaryExtractor.ExtractMaxSeatsFlag(_skillStaffPeriodList.ToList(),
					TimeZoneGuard.Instance.TimeZone)).Return(result);
			}
			using (_mock.Playback())
			{
				_target.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 05, 28), _schedulingResultStateHolder,
					TimeZoneGuard.Instance.TimeZone);
			}
		}
	}
}
