using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers
{
	public class PersonResolver : IPersonResolver
	{
		private readonly IDatabaseReader _databaseReader;
		
		public PersonResolver(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<PersonWithBusinessUnit> personId)
		{
			var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, logOn).ToUpper(CultureInfo.InvariantCulture);
			if (string.IsNullOrEmpty(logOn))
			{
				lookupKey = string.Empty;
			}
			
			var dictionary = _databaseReader.ExternalLogOns();
			return dictionary.TryGetValue(lookupKey, out personId);
		}
	}
}