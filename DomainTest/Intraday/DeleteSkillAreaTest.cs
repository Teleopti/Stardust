using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class DeleteSkillAreaTest
	{
		public DeleteSkillArea Target;
		public FakeSkillAreaRepository SkillAreaRepository;

		[Test]
		public void ShouldDelete()
		{
			var skillArea = new SkillArea().WithId();
			SkillAreaRepository.Has(skillArea);

			SkillAreaRepository.LoadAll().Count.Should().Be.EqualTo(1);

			Target.Do(skillArea.Id.Value);

			SkillAreaRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}
	}
}