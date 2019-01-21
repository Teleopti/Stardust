using System;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		public TeleoptiIdentity(string name,
			IDataSource dataSource,
			Guid? businessUnitId,
			string businessUnitName,
			WindowsIdentity windowsIdentity,
			string tokenIdentity)
			: base(name)
		{
			DataSource = dataSource;
			BusinessUnitId = businessUnitId;
			BusinessUnitName = businessUnitName;
			WindowsIdentity = windowsIdentity;
			TokenIdentity = tokenIdentity;
		}

		public Guid? BusinessUnitId { get; set; }
		public string BusinessUnitName { get; set; }
		public string TokenIdentity { get; set; }
		public IDataSource DataSource { get; set; }
		public WindowsIdentity WindowsIdentity { get; set; }
	}
}