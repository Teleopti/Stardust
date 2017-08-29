using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// SkillTypeFactory
    /// </summary>
    public static class SkillTypeFactory
    {
        /// <summary>
        /// Creates a SkillType for Inbound Telephony
        /// </summary>
        /// <returns></returns>
        public static SkillType CreateSkillType()
        {
            Description desc = new Description("My Phone skill type");

            return new SkillTypePhone(desc, ForecastSource.InboundTelephony);
        }

        /// <summary>
        /// Creates a SkillType for Backoffice
        /// </summary>
        /// <returns></returns>
        public static SkillType CreateSkillTypeBackoffice()
        {
            Description desc = new Description("My Backoffice skill type");

            return new SkillTypePhone(desc, ForecastSource.Backoffice);
        }

        /// <summary>
        /// Creates a SkillType for Email
        /// </summary>
        /// <returns></returns>
        public static SkillType CreateSkillTypeEmail()
        {
            Description desc = new Description("My Email skill type");

            return new SkillTypeEmail(desc, ForecastSource.Email);
        }
    }
}