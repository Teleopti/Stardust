using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Intraday.UnitTests
{
	[DomainTest]
	public class SkillTypeInfoProviderTest
	{
		public ISkillTypeInfoProvider Target;

		[Test]
		public void ShouldReturnSupportAbandonRateAndReforcastedForPhoneTypeSkill()
		{
			var phoneSkill = SkillFactory.CreateSkill("Phone",
				new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony), 15);

			var skillTypeInfo = Target.GetSkillTypeInfo(phoneSkill);

			skillTypeInfo.SupportsAbandonRate.Should().Be.True();
			skillTypeInfo.SupportsReforecastedAgents.Should().Be.True();
		}

		[Test]
		public void ShouldNotReturnSupportAbandonRateAndReforcastedForEmailTypeSkill()
		{
			var emailSkill = SkillFactory.CreateSkill("Email",
				new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email), 15);

			var skillTypeInfo = Target.GetSkillTypeInfo(emailSkill);

			skillTypeInfo.SupportsAbandonRate.Should().Be.False();
			skillTypeInfo.SupportsReforecastedAgents.Should().Be.False();

		}
	}
}
