using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SkillTypeFactory
    {
        public static SkillType CreateSkillTypePhone()
        {
            return new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony);
        }

        public static SkillType CreateSkillTypeBackoffice()
        {
            return new SkillTypePhone(new Description(SkillTypeIdentifier.BackOffice), ForecastSource.Backoffice);
        }

        public static SkillType CreateSkillTypeEmail()
        {
			return new SkillTypePhone(new Description(SkillTypeIdentifier.Email), ForecastSource.Email);
		}
    }
}