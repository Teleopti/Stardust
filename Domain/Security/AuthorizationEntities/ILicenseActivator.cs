using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

	public interface ILicenseActivator
	{

		string EnabledLicenseSchemaName { get; }

		string CustomerName { get; }

		DateTime ExpirationDate { get; }

		int MaxActiveAgents { get; }

		Percent MaxActiveAgentsGrace { get; }

		bool IsThisTooManyActiveAgents(int activeAgents);

		bool IsThisAlmostTooManyActiveAgents(int activeAgents);

		IList<string> EnabledLicenseOptionPaths { get; }

		int MaxSeats { get; }

		LicenseType LicenseType { get; }
		string MajorVersion { get; }
	}
}