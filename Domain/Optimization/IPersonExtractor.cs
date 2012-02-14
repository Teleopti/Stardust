using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts the list of skills
    /// </summary>
    public interface IPersonExtractor
    {
        IEnumerable<IPerson> ExtractPersons();
    }
}