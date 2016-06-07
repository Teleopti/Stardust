using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLicenseActivator : ILicenseActivator
	{
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
		public bool IsThisTooManyActiveAgents(int activeAgents)
		{
			throw new NotImplementedException();
		}

		public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
		{
			throw new NotImplementedException();
		}

		public IList<string> EnabledLicenseOptionPaths { get; }
		public int MaxSeats { get; }
		public LicenseType LicenseType { get; }
		public string MajorVersion { get; }
	}
}