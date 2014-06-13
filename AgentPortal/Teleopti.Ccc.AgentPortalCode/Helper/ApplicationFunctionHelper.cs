using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public interface IApplicationFunctionHelper
    {
        DefinedRaptorApplicationFunctionPathsDto DefinedApplicationFunctionPaths { get; }
    }

    public class ApplicationFunctionHelper : IApplicationFunctionHelper
    {
        private static ApplicationFunctionHelper _instance;

        public static ApplicationFunctionHelper Instance()
        {
            if (_instance == null)
            {
                _instance = new ApplicationFunctionHelper();
            }
            return _instance;
        }

        public ApplicationFunctionHelper()
        {
            DefinedApplicationFunctionPaths = SdkServiceHelper.LogOnServiceClient.GetDefinedApplicationFunctionPaths();
        }

        public DefinedRaptorApplicationFunctionPathsDto DefinedApplicationFunctionPaths { get; private set; }
    }
}