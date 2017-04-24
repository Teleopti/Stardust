using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Creates new workshifts from one template,
    /// based on extenders and limiters
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-18
    /// </remarks>
    public class CreateWorkShiftsFromTemplate : ICreateWorkShiftsFromTemplate
    {
		public WorkShiftCollection Generate(IWorkShift shiftTemplate, IList<IWorkShiftExtender> extenders, IList<IWorkShiftLimiter> limiters,
	                                         IWorkShiftAddCallback callback)
	    {
			if (tryJumpOutEarly(limiters, shiftTemplate, extenders))
				return new WorkShiftCollection(callback);

			var workingList = new WorkShiftCollection(callback) { shiftTemplate };
			foreach (var extender in extenders)
			{
				if (callback != null && callback.IsCanceled)
					break;
				workingList = generateForOneExtender(workingList, extender);
			}

			removeInvalidAtEnd(workingList, limiters,callback);
			return workingList;
	    }

	    private static bool tryJumpOutEarly(IEnumerable<IWorkShiftLimiter> limiters, IWorkShift shiftTemplate, IList<IWorkShiftExtender> extenders)
        {
            return !limiters.All(limiter => limiter.IsValidAtStart(shiftTemplate, extenders));
        }

		private static void removeInvalidAtEnd(WorkShiftCollection workingList, ICollection<IWorkShiftLimiter> limiters, IWorkShiftAddCallback callback)
        {
            if(limiters.Count>0)
            {
                for (int i = workingList.Count - 1; i >= 0; i--)
                {
					if (callback != null && callback.IsCanceled)
						break;
                    var shift = workingList[i];
                    var shiftProjection = shift.ProjectionService().CreateProjection();
                    foreach (var limiter in limiters)
                    {
						if (callback != null && callback.IsCanceled)
							break;
	                    if (limiter.IsValidAtEnd(shiftProjection)) continue;
	                    workingList.RemoveAt(i);
	                    break;
                    }
                }                
            }
        }

		private static WorkShiftCollection generateForOneExtender(WorkShiftCollection workingList,
                                                                IWorkShiftExtender extender)
        {
            var returnCollection = new WorkShiftCollection(workingList.Callback);

            foreach (var shift in workingList)
            {
				if (workingList.Callback != null)
				{
					workingList.Callback.BeforeRemove();
					if (workingList.Callback.IsCanceled)
						break;
				}
				
	            var tempShifts = extender.ReplaceWithNewShifts(shift);
	            foreach (var tempShift in tempShifts)
	            {
					returnCollection.Add(tempShift);
	            }
            }
            return returnCollection;
        }
    }
}