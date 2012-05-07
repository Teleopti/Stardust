using System;
using System.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiIdentity : IIdentity
	{
		WindowsIdentity WindowsIdentity { get; }
		AuthenticationTypeOption TeleoptiAuthenticationType { get; }
		IDataSource DataSource { get; }
		IBusinessUnit BusinessUnit { get; }
		Guid BusinessUnitId { get; }
		string Ticket { get; set; }
	}
}