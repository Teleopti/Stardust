using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SkillGroups
{
	[DomainTest]
	public class DeleteSkillGroupTest
	{
		public DeleteSkillGroup Target;
		public FakeSkillGroupRepository SkillGroupRepository;

		[Test]
		public void ShouldDelete()
		{
			var skillArea = new SkillGroup().WithId();
			SkillGroupRepository.Has(skillArea);

			SkillGroupRepository.LoadAll().Count.Should().Be.EqualTo(1);

			Target.Do(skillArea.Id.Value);

			SkillGroupRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}
	}
}