using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class MaxSeatSkillAggregatorTest
	{
		private IMaxSeatSkillAggregator _target;
		private IList<IPerson  > _teamMembers;
		private DateOnlyPeriod  _dateOnlyPeriod;
		private IPerson _person1;
		private IPerson _person2;
		private MockRepository _mock;
		private IPersonPeriod _personPeriod1;
		private IPersonPeriod _personPeriod2;
		private IList<IPersonSkill> _skillList;
		private ISkill _skill1;

		[SetUp]
		public void Setup()
		{
			_target = new MaxSeatSkillAggregator();
			_mock = new MockRepository();
			_person1 = _mock.StrictMock<IPerson>();
			_person2 = _mock.StrictMock<IPerson>();
			_personPeriod1  = _mock.StrictMock<IPersonPeriod>();
			_personPeriod2 = _mock.StrictMock<IPersonPeriod>();
			_skill1 = SkillFactory.CreateSkill("skill1");
			_skillList = new List<IPersonSkill> {PersonSkillFactory.CreatePersonSkill(_skill1,20)};
		}

		[Test]
		public void ShouldReturnSkillOnProvidedDate()
		{
			_teamMembers = new List<IPerson>();
			_teamMembers.Add(_person1 );
			_teamMembers.Add(_person2);
			_dateOnlyPeriod = new DateOnlyPeriod(2014, 05, 26, 2014, 05, 26);
			IList<IPersonPeriod> person1PersonPeriodList = new List<IPersonPeriod>{_personPeriod1};
			IList<IPersonPeriod> person2PersonPeriodList = new List<IPersonPeriod>{_personPeriod2};
			using (_mock.Record())
			{
				Expect.Call(_person1.PersonPeriods(_dateOnlyPeriod)).Return(person1PersonPeriodList);
				Expect.Call(_personPeriod1.PersonMaxSeatSkillCollection).Return(_skillList);
				Expect.Call(_person2.PersonPeriods(_dateOnlyPeriod)).Return(person2PersonPeriodList);
				Expect.Call(_personPeriod2.PersonMaxSeatSkillCollection).Return(_skillList);
			}
			HashSet<ISkill> aggregatedSkills = _target.GetAggregatedSkills(_teamMembers, _dateOnlyPeriod);
			using (_mock.Playback())
			{
				Assert.AreEqual(1,aggregatedSkills.Count( ));
				Assert.AreEqual(_skill1,aggregatedSkills.ToList()[0] );
			}
		}

		[Test]
		public void ShouldReturnEmptyListIfNoPersonPeriod()
		{
			_teamMembers = new List<IPerson>();
			_teamMembers.Add(_person1);
			_teamMembers.Add(_person2);
			_dateOnlyPeriod = new DateOnlyPeriod(2014, 05, 26, 2014, 05, 26);
			using (_mock.Record())
			{
				Expect.Call(_person1.PersonPeriods(_dateOnlyPeriod)).Return(new List<IPersonPeriod>( ));
				Expect.Call(_person2.PersonPeriods(_dateOnlyPeriod)).Return(new List<IPersonPeriod>());
			}
			HashSet<ISkill> aggregatedSkills = _target.GetAggregatedSkills(_teamMembers, _dateOnlyPeriod);
			using (_mock.Playback())
			{
				Assert.AreEqual(0, aggregatedSkills.Count());
			}
		}



	}

	
}
