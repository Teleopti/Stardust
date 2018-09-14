using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
    public interface ISearchPath
	{
        string Path { get; }
	    string PayrollDeployNewPath { get; }

	}
}