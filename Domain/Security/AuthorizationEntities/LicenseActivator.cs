using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public delegate bool IsThisTooManyActiveAgents(int maxLicensedActiveAgents, Percent maxActiveAgentsGrace, int activeAgents);

	public delegate bool IsThisAlmostTooManyActiveAgents(int maxLicensedActiveAgents, int activeAgents);

	/// <summary>
	/// License activator class. Activates the license schema.
	/// </summary>
	public class LicenseActivator : ILicenseActivator
	{

		public static bool IsThisAlmostTooManyActiveAgents(int maxLicensedActiveAgents, int activeAgents)
		{
			return XmlLicenseService.IsThisAlmostTooManyActiveAgents(maxLicensedActiveAgents, activeAgents);
		}

		public static bool IsThisTooManyActiveAgents(int maxLicensedActiveAgents, Percent maxActiveAgentsGrace, int activeAgents)
		{
			return XmlLicenseService.IsThisTooManyActiveAgents(maxLicensedActiveAgents, maxActiveAgentsGrace.Value, activeAgents);
		}

		public LicenseActivator(string customerName, DateTime expirationDate, int maxActiveAgents, int maxSeats, LicenseType licenseType, Percent maxActiveAgentsGrace,
				IsThisAlmostTooManyActiveAgents isThisAlmostTooManyActiveAgents, IsThisTooManyActiveAgents isThisTooManyActiveAgents)
		{
			CustomerName = customerName;
			ExpirationDate = expirationDate;
			MaxActiveAgents = maxActiveAgents;
			MaxSeats = maxSeats;
			LicenseType = licenseType;
			MaxActiveAgentsGrace = maxActiveAgentsGrace;
			_isThisTooManyActiveAgents = isThisTooManyActiveAgents;
			_isThisAlmostTooManyActiveAgents = isThisAlmostTooManyActiveAgents;
		}


		private readonly IList<string> _enabledLicenseOptionPaths = new List<string>();

		private readonly IsThisTooManyActiveAgents _isThisTooManyActiveAgents;

		private readonly IsThisAlmostTooManyActiveAgents _isThisAlmostTooManyActiveAgents;

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

		public DateTime ExpirationDate { get; private set; }

		public int MaxActiveAgents { get; private set; }

		public int MaxSeats { get; private set; }

		public LicenseType LicenseType { get; private set; }

		public Percent MaxActiveAgentsGrace { get; private set; }

		public bool IsThisTooManyActiveAgents(int activeAgents)
		{
			return _isThisTooManyActiveAgents(MaxActiveAgents, MaxActiveAgentsGrace, activeAgents);
		}

		public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
		{
			return _isThisAlmostTooManyActiveAgents(MaxActiveAgents, activeAgents);
		}

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
