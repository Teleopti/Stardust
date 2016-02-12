using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class CreateSkillAreaTest
	{
		public CreateSkillArea Target;
		public FakeSkillAreaRepository SkillAreaRepository;

		[Test]
		public void ShouldCreate()
		{
			var newGuid = Guid.NewGuid();
			const string name = "new skill area";
			Target.Create(name, new[] {newGuid});
			SkillAreaRepository.LoadAll()
				.First(x => x.Name == name)
				.Skills.First().Id
				.Should().Be.EqualTo(newGuid);
		}
	}
}