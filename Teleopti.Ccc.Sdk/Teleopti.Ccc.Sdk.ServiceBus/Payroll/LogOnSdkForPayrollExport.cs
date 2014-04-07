using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Messages.Payroll;
using log4net;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class LogOnSdkForPayrollExport : ILogOnSdkForPayrollExport
    {
        private readonly IChannelCreator _channelCreator;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(PayrollExportConsumer));

        public LogOnSdkForPayrollExport(IChannelCreator channelCreator)
        {
            _channelCreator = channelCreator;
        }

        public void LogOn(RunPayrollExport message)
        {
            var logonService = _channelCreator.CreateChannel<ITeleoptiCccLogOnService>();

            var datasources = logonService.GetDataSources();
            var datasource = datasources.FirstOrDefault(d => d.Name == message.Datasource);
            
            if (datasource == null)
            {
                Logger.ErrorFormat("Cannot find datasource: {0}", message.Datasource);
            }

            var authenticationResult = logonService.LogOnApplicationUser(SuperUser.UserName, SuperUser.Password, datasource);
            
            if (!authenticationResult.Successful)
            {
                Logger.ErrorFormat(CultureInfo.CurrentCulture,"Authentication failed");
            }

            var businessunit = authenticationResult.BusinessUnitCollection.FirstOrDefault(
                    b => b.Id.GetValueOrDefault() == message.BusinessUnitId);
#pragma warning disable 612,618
            logonService.SetBusinessUnit(businessunit);
#pragma warning restore 612,618
        }
    }
}