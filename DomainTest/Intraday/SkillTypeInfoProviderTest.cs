using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class SkillTypeInfoProviderTest
	{
		public ISkillTypeInfoProvider Target;

		[Test]
		[Toggle(Toggles.WFM_Intraday_SupportOtherSkillsLikeEmail_44026)]
		public void ShouldReturnSupportAbandonRateAndReforcastedForPhoneTypeSkill()
		{
			var phoneSkill = SkillFactory.CreateSkill("Phone",
				new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony), 15);

			var skillTypeInfo = Target.GetSkillTypeInfo(phoneSkill);

			skillTypeInfo.SupportsAbandonRate.Should().Be.True();
			skillTypeInfo.SupportsReforecastedAgents.Should().Be.True();
		}

		[Test]
		[Toggle(Toggles.WFM_Intraday_SupportOtherSkillsLikeEmail_44026)]
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
