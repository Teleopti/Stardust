using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class LicenseActivator : ILicenseActivator
	{
		public LicenseActivator(string customerName, DateTime expirationDate, bool perpetual, int maxActiveAgents, int maxSeats, LicenseType licenseType, Percent maxActiveAgentsGrace, string majorVersion)
		{
			CustomerName = customerName;
			ExpirationDate = expirationDate;
			Perpetual = perpetual;
			MaxActiveAgents = maxActiveAgents;
			MaxSeats = maxSeats;
			LicenseType = licenseType;
			MaxActiveAgentsGrace = maxActiveAgentsGrace;
			MajorVersion = majorVersion;
		}


		private readonly IList<string> _enabledLicenseOptionPaths = new List<string>();


		public string EnabledLicenseSchemaName
		{
			get
			{
				if (_enabledLicenseOptionPaths.Count > 0)
				{
					return GetSchemaName(_enabledLicenseOptionPaths[0]);
				}
				return string.Empty;
			}
		}

		public string CustomerName { get; private set; }

		public string MajorVersion { get; private set; }

		public DateTime ExpirationDate { get; private set; }
		public bool Perpetual { get; private set; }

		public int MaxActiveAgents { get; private set; }

		public int MaxSeats { get; private set; }

		public LicenseType LicenseType { get; private set; }

		public Percent MaxActiveAgentsGrace { get; private set; }

		public IList<string> EnabledLicenseOptionPaths
		{
			get { return _enabledLicenseOptionPaths; }
		}

		protected static string GetSchemaName(string optionPath)
		{
			int lastSeparator = optionPath.LastIndexOf("/", StringComparison.Ordinal);
			if (lastSeparator == -1)
				return string.Empty;
			return optionPath.Substring(0, lastSeparator);
		}

	}
}
