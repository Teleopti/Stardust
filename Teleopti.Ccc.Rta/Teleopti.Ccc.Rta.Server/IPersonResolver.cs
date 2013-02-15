using System.Collections.Generic;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IPersonResolver
    {
        bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<PersonWithBusinessUnit> personId);
    }
}