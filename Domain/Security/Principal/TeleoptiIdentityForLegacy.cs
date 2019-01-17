using System;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class ITeleoptiIdentityForLegacyExtensions	
	{
		public static IBusinessUnit BusinessUnit(this ITeleoptiIdentityForLegacy instance)
		{
			return instance.UnsafeBusinessUnitObject as IBusinessUnit;
		}
	}

	public class TeleoptiIdentityForLegacy : GenericIdentity, ITeleoptiIdentity, ITeleoptiIdentityForLegacy
	{
		private IBusinessUnit _businessUnit;

		public TeleoptiIdentityForLegacy(
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
		
		public object UnsafeBusinessUnitObject => _businessUnit;
	}
}