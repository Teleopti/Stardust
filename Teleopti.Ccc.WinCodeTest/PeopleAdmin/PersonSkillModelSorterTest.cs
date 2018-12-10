using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
	[TestFixture]
	public class PersonSkillModelSorterTest
	{

		[Test]
		public void ShouldSort()
		{
			var personSkillModelSorter = new PersonSkillModelSorter();
			var personSkillModelAllHave1 = new PersonSkillModel {TriState = 1, ContainedEntity = new PersonSkill(new Skill("A_AllHave"), new Percent()).WithId()};
			var personSkillModelAllHave2 = new PersonSkillModel {TriState = 1, ContainedEntity = new PersonSkill(new Skill("B_AllHave"), new Percent()).WithId()};
			var personSkillModelSomeHave1 = new PersonSkillModel {TriState = 2, ContainedEntity = new PersonSkill(new Skill("A_SomeHave"), new Percent()).WithId()};
			var personSkillModelSomeHave2 = new PersonSkillModel {TriState = 2, ContainedEntity = new PersonSkill(new Skill("B_SomeHave"), new Percent()).WithId()};
			var personSkillModelNoneHave1 = new PersonSkillModel {TriState = 0, ContainedEntity = new PersonSkill(new Skill("A_NoneHave"), new Percent()).WithId()};
			var personSkillModelNoneHave2 = new PersonSkillModel {TriState = 0, ContainedEntity = new PersonSkill(new Skill("B_NoneHave"), new Percent()).WithId()};
			var personSkillModels = new List<PersonSkillModel>
			{
				personSkillModelNoneHave2,
				personSkillModelAllHave2,
				personSkillModelSomeHave1,
				personSkillModelNoneHave1,
				personSkillModelAllHave1,
				personSkillModelSomeHave2
			};

			var result = personSkillModelSorter.Sort(personSkillModels).ToList();

			result[0].Should().Be.EqualTo(personSkillModelAllHave1);
			result[1].Should().Be.EqualTo(personSkillModelAllHave2);
			result[2].Should().Be.EqualTo(personSkillModelSomeHave1);
			result[3].Should().Be.EqualTo(personSkillModelSomeHave2);
			result[4].Should().Be.EqualTo(personSkillModelNoneHave1);
			result[5].Should().Be.EqualTo(personSkillModelNoneHave2);
		}
	}
}
