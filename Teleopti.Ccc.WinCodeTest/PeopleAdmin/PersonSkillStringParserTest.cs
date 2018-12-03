using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
	[TestFixture]
	public class PersonSkillStringParserTest
	{
		private MockRepository _mocks;
		private ISkill _skill1;
		private ISkill _skill2;
		private PersonSkill _personSkill1;
		private PersonSkill _personSkill2;
		private List<IPersonSkill> _personSkills;
		private PersonSkillStringParser _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skill1 = _mocks.StrictMock<ISkill>();
			_skill2 = _mocks.StrictMock<ISkill>();
			_personSkill1 = new PersonSkill(_skill1, new Percent(1));
			_personSkill2 = new PersonSkill(_skill2, new Percent(1));
			_personSkills = new List<IPersonSkill> {_personSkill1, _personSkill2};

			_target = new PersonSkillStringParser(_personSkills);

		}

		[Test]
		public void ShouldReturnListWithUniqueSkills()
		{
			Expect.Call(_skill1.Name).Return("skill1").Repeat.Any();
			Expect.Call(_skill2.Name).Return("skill2").Repeat.Any();
			_mocks.ReplayAll();
			var ret = _target.Parse("skill1,skill1, skill2");
			Assert.That(ret.Count, Is.EqualTo(2));
			Assert.That(ret[0].Active,Is.True);
			_mocks.VerifyAll();
		}
	}
}