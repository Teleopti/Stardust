using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    public class TreeFamily
    {
        private readonly string _applicationFunctionPath;

        public TreeFamily(string userText)
        {
            _applicationFunctionPath = DefinedRaptorApplicationFunctionPaths.OpenOptionsPage;
            UserText = userText;
        }

        public TreeFamily(string userText, string applicationFunctionPath)
        {
            _applicationFunctionPath = applicationFunctionPath;
            UserText = userText;
        }
        public string UserText { get; private set; }

        public bool CheckPermission()
        {
            return PrincipalAuthorization.Current_DONTUSE().IsPermitted(_applicationFunctionPath);
        }
    }
}
