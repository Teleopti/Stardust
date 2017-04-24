using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Creates workshift based on a template.
    /// Used by ShiftCreatorService
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-18
    /// </remarks>
    public interface ICreateWorkShiftsFromTemplate
    {
	    /// <summary>
	    /// Generates workshift based on the specified template.
	    /// </summary>
	    /// <param name="shiftTemplate">The template.</param>
	    /// <param name="extenders">The extenders.</param>
	    /// <param name="limiters">The limiters.</param>
	    /// <param name="callback"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: rogerkr
	    /// Created date: 2008-03-18
	    /// </remarks>
	    WorkShiftCollection Generate(IWorkShift shiftTemplate, IList<IWorkShiftExtender> extenders, IList<IWorkShiftLimiter> limiters, IWorkShiftAddCallback callback);
    }
}