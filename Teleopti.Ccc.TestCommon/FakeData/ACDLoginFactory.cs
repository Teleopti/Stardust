using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// ExternalLogOnFactory factory
    /// </summary>
    public static class ExternalLogOnFactory
    {
        /// <summary>
        /// Creates an ACD Login
        /// </summary>
        /// <returns></returns>
        public static ExternalLogOn CreateExternalLogOn()
        {
            string name = "Login name";
            string code = "CODE";
            int loginId = 17;
            int loginAggId = 19;

            return new ExternalLogOn(loginId,loginAggId,code,name,true){DataSourceId = 3};
        }
    }
}