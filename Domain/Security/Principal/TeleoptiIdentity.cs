using System;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		private IBusinessUnit _businessUnit;

		public TeleoptiIdentity(
			string name,
			IDataSource dataSource,
			IBusinessUnit businessUnit,
			WindowsIdentity windowsIdentity,
			string tokenIdentity)
			: base(name)
		{
			DataSource = dataSource;
			_businessUnit = businessUnit;
			WindowsIdentity = windowsIdentity;
			TokenIdentity = tokenIdentity;
		}

		public Guid? BusinessUnitId => _businessUnit?.Id.GetValueOrDefault();
		public string BusinessUnitName => _businessUnit?.Name;
		public string TokenIdentity { get; set; }
		public IDataSource DataSource { get; set; }
		public WindowsIdentity WindowsIdentity { get; set; }
	}
}