using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface IDayOffLegalStateValidatorListCreator
    {
        /// <summary>
        /// Builds the <see cref="IDayOffLegalStateValidator"/> active validator list.
        /// </summary>
        /// <returns></returns>
        IList<IDayOffLegalStateValidator> BuildActiveValidatorList();
    }
}