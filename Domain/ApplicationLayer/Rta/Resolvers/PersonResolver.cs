using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers
{
	public class PersonResolver
	{
		private readonly IDatabaseLoader _databaseReader;
		
		public PersonResolver(IDatabaseLoader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<ResolvedPerson> personId)
		{
			var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, logOn).ToUpperInvariant();
			if (string.IsNullOrEmpty(logOn))
			{
				lookupKey = string.Empty;
			}
			
			var dictionary = _databaseReader.ExternalLogOns();
			return dictionary.TryGetValue(lookupKey, out personId);
		}
	}

}