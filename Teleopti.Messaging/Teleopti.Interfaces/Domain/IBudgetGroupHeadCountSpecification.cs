using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A specification for checking head counts allowance in the budget group
    /// </summary>
    public interface IBudgetGroupHeadCountSpecification: ISpecification<IAbsenceRequest>
    {
    }
}
