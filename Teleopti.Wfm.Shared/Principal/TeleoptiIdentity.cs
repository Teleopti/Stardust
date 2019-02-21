using System;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		private readonly Func<Guid?> _businessUnitId = () => null;

		public TeleoptiIdentity(string name,
			IDataSource dataSource,
			Func<Guid?> businessUnitId,
			string businessUnitName,
			WindowsIdentity windowsIdentity,
			string tokenIdentity)
			: base(name)
		{
			_businessUnitId = businessUnitId ?? _businessUnitId;
			BusinessUnitName = businessUnitName;
			DataSource = dataSource;
			WindowsIdentity = windowsIdentity;
			TokenIdentity = tokenIdentity;
		}

		public Guid? BusinessUnitId => _businessUnitId.Invoke();
		public string BusinessUnitName { get; }
		public string TokenIdentity { get; set; }
		public IDataSource DataSource { get; set; }
		public WindowsIdentity WindowsIdentity { get; set; }
	}
}