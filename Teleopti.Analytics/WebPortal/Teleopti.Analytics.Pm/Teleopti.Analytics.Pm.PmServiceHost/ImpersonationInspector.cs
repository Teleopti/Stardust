using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    public class ImpersonationInspector : IParameterInspector
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImpersonationInspector));

        public object BeforeCall(string operationName, object[] inputs)
        {
            Logger.DebugFormat("BeforeCall to '{0}'", operationName);
            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            Logger.DebugFormat("AfterCall to '{0}'", operationName);
        }
    }
}
