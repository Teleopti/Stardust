using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SkillGroup;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SkillGroups
{
	[DomainTest]
	public class CreateSkillGroupTest
	{
		public CreateSkillGroup Target;
		public FakeSkillGroupRepository SkillGroupRepository;

		[Test]
		public void ShouldCreate()
		{
			var newGuid = Guid.NewGuid();
			const string name = "new skill area";
			Target.Create(name, new[] {newGuid});
			SkillGroupRepository.LoadAll()
				.First(x => x.Name == name)
				.Skills.First().Id
				.Should().Be.EqualTo(newGuid);
		}
	}
}