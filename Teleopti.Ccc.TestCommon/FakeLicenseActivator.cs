using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;


namespace Teleopti.Ccc.TestCommon
{
	public class FakeLicenseActivator : ILicenseActivator
	{
		public FakeLicenseActivator(string customerName) : this()
		{
			CustomerName = customerName;
		}
		public FakeLicenseActivator()
		{
			EnabledLicenseOptionPaths = new List<string>();
		}

		public string EnabledLicenseSchemaName { get; }
		public string CustomerName { get; }
		public DateTime ExpirationDate { get; }
		public bool Perpetual { get; }
		public int MaxActiveAgents { get; }
		public Percent MaxActiveAgentsGrace { get; }
		public IList<string> EnabledLicenseOptionPaths { get; }
		public int MaxSeats { get; }
		public LicenseType LicenseType { get; }
		public string MajorVersion { get; }
	}
}