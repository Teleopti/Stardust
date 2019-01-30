using System;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class TeleoptiIdentityWithUnsafeBusinessUnitExtensions	
	{
		public static IBusinessUnit BusinessUnit(this ITeleoptiIdentityWithUnsafeBusinessUnit instance)
		{
			return instance.UnsafeBusinessUnitObject() as IBusinessUnit;
		}
	}

	public class TeleoptiIdentityWithUnsafeBusinessUnit : GenericIdentity, ITeleoptiIdentity, ITeleoptiIdentityWithUnsafeBusinessUnit
	{
		private IBusinessUnit _businessUnit;

		public TeleoptiIdentityWithUnsafeBusinessUnit(
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

		public object UnsafeBusinessUnitObject() => _businessUnit;
	}
}