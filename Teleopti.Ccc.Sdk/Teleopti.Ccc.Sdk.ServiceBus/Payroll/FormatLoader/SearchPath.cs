using System;
using System.IO;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public class SearchPath : ISearchPath
    {
		public string Path => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Payroll\");

	    public string PayrollDeployNewPath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Payroll.DeployNew\");
    }

	public class FakeSearchPath : ISearchPath
	{

		public string Path
		{
			get
			{
				var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(GetType()).Location);
				return System.IO.Path.Combine(path, @"Payroll\");
			}
		}

		public string PayrollDeployNewPath
		{
			get
			{
				var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(GetType()).Location);
				return System.IO.Path.Combine(path, @"Payroll.DeployNew\");
				
			}
		}
		
		
	}
}