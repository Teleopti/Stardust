using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class FetchSkillAreaTest
	{
		public FetchSkillArea Target;
		public FakeSkillAreaRepository SkillAreaRepository;

		[Test]
		public void ShouldGetAll()
		{
			var existingSkillArea = new SkillArea();
			SkillAreaRepository.Has(existingSkillArea);

			Target.GetAll().Should().Have.SameValuesAs(existingSkillArea);
		}
	}
}