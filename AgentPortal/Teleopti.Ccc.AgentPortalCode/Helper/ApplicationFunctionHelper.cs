using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public interface IApplicationFunctionHelper
    {
        DefinedRaptorApplicationFunctionPathsDto DefinedApplicationFunctionPaths { get; }
        bool IsUnderConstructionVisible();
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

        public bool IsUnderConstructionVisible()
        {
            var applicationFunctionDtos =
                SdkServiceHelper.LogOnServiceClient.GetApplicationFunctionsForPerson(
                    StateHolder.Instance.State.SessionScopeData.LoggedOnPerson);

            foreach (var dto in applicationFunctionDtos)
            {
                if (dto.FunctionCode == "UnderConstruction")
                {
                    return !dto.IsPreliminary;
                }
            }
            return false;
        }
    }
}