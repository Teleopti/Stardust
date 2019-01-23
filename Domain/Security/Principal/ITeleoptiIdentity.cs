using System;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiIdentity : IIdentity
	{
		WindowsIdentity WindowsIdentity { get; }
		IDataSource DataSource { get; }
		Guid? BusinessUnitId { get; }
		string BusinessUnitName { get; }
		string TokenIdentity { get; set; }
	}
}