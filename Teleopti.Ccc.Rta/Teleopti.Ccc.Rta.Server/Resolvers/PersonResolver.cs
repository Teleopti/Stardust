using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.Rta.Server.Resolvers
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
			
			var dictionary = _databaseReader.LoadAllExternalLogOns();
			addBatchSignature(dictionary);
			return dictionary.TryGetValue(lookupKey, out personId);
		}

		private static void addBatchSignature(ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> dictionary)
		{
			IEnumerable<PersonWithBusinessUnit> foundIds;
			if (dictionary.TryGetValue(string.Empty, out foundIds)) return;
			var emptyArray = new[] { new PersonWithBusinessUnit { PersonId = Guid.Empty, BusinessUnitId = Guid.Empty } };
			dictionary.AddOrUpdate(string.Empty, emptyArray, (s, units) => emptyArray);
		}
	}
}