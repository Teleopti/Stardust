using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class SearchPath : ISearchPath
    {
        public string Path => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Payroll\");

	    public string PayrollDeployNewPath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Payroll.DeployNew\");
    }
}