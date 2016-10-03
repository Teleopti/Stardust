using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[Ignore(""),DomainTest]
	public class CalculateResourceReadModelTest : ISetup
	{
		public CalculateResourceReadModel Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<CalculateResourceReadModel>().For<CalculateResourceReadModel>();
		}

		[Test]
		public void ShouldPerformResourceCalculation()
		{
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodExt = new SkillSkillStaffPeriodExtendedDictionary();
			Target.ResourceCalculatePeriod(new DateTimePeriod());
		}
	}
}
