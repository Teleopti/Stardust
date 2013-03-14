using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBudgetGroupAllowanceCalculator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="absenceRequest"></param>
        /// <returns></returns>
        string CheckBudgetGroup(IAbsenceRequest absenceRequest);
    }
}
