using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using log4net;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class InitializePayrollFormats : IInitializePayrollFormats
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InitializePayrollFormats));
        private readonly IPlugInLoader _plugInLoader;
        
        public InitializePayrollFormats(IPlugInLoader plugInLoader)
        {
            _plugInLoader = plugInLoader;
        }

        public void Initialize()
        {
            using (var proxy = new Proxy())
            {
                try
                {
                    Logger.Info("Initializing payroll formats");
                    
                    var folder = AppDomain.CurrentDomain.BaseDirectory;
                    Logger.InfoFormat("Loading plugins in folder {0}", folder);
                    var payrollExportProcessors = _plugInLoader.Load();
                    var allPayrollFormats = payrollExportProcessors.Select(p => p.PayrollFormat);
                    Logger.InfoFormat(CultureInfo.CurrentCulture, "Sending formats to SDK");
                    proxy.InitializePayrollFormats(allPayrollFormats.ToList());
                }
                catch (CommunicationException exception)
                {
                    Logger.ErrorFormat("Error when initializing payroll formats: {0}", exception.Message);
                }
            }
        }
    }
}
