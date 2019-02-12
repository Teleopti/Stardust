using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class PersistedTypeMapping
	{
		public string CurrentPersistedName;
		public IEnumerable<string> LegacyPersistedNames = Enumerable.Empty<string>();

		public string CurrentTypeName;
	}
}