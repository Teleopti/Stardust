using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiIdentity : IIdentity
	{
		WindowsIdentity WindowsIdentity { get; }
		IDataSource DataSource { get; }
		IBusinessUnit BusinessUnit { get; }
		string TokenIdentity { get; set; }
	}
}