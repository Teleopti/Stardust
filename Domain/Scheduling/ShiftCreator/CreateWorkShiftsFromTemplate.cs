using System.Linq;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
		public IList<IWorkShift> Generate(IWorkShift shiftTemplate, IList<IWorkShiftExtender> extenders, IList<IWorkShiftLimiter> limiters,
	                                         IWorkShiftAddCallback callback)
	    {
			if (tryJumpOutEarly(limiters, shiftTemplate, extenders))
				return new List<IWorkShift>();

			var workingList = new WorkShiftCollection(callback) { shiftTemplate };
			foreach (var extender in extenders)
			{
				if (callback != null && callback.IsCanceled)
					break;
				workingList = generateForOneExtender(workingList, extender,limiters);
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
                    IWorkShift shift = workingList[i];
                    IVisualLayerCollection shiftProjection = shift.ProjectionService().CreateProjection();
                    foreach (IWorkShiftLimiter limiter in limiters)
                    {
						if (callback != null && callback.IsCanceled)
							break;
                        if (!limiter.IsValidAtEnd(shiftProjection))
                        {
                            workingList.RemoveAt(i);
                            break;
                        }
                    }
                }                
            }
        }

		private static bool checkLimiters(IWorkShift shift, IEnumerable<IWorkShiftLimiter> limiters)
		{
			var shiftProjection = shift.ProjectionService().CreateProjection();
			return limiters.All(limiter => limiter.IsValidAtEnd(shiftProjection));
		}

		private static WorkShiftCollection generateForOneExtender(WorkShiftCollection workingList,
                                                                IWorkShiftExtender extender,
																IList<IWorkShiftLimiter> limiters)
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
					//if(checkLimiters(tempShift,limiters))
						returnCollection.Add(tempShift);
	            }
                //returnCollection.AddRange();
            }
            return returnCollection;
        }
    }
}