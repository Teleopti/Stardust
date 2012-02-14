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
        public IList<IWorkShift> Generate(IWorkShift shiftTemplate, 
                                            IList<IWorkShiftExtender> extenders,
                                            IList<IWorkShiftLimiter> limiters)
        {
            if(tryJumpOutEarly(limiters, shiftTemplate, extenders))
                return new List<IWorkShift>();

            IList<IWorkShift> workingList = new List<IWorkShift> { shiftTemplate };
            foreach (IWorkShiftExtender extender in extenders)
            {
                workingList = generateForOneExtender(workingList, extender);
            }

            removeInvalidAtEnd(workingList, limiters);
            return workingList;
        }

        private static bool tryJumpOutEarly(IEnumerable<IWorkShiftLimiter> limiters, IWorkShift shiftTemplate, IList<IWorkShiftExtender> extenders)
        {
            return !limiters.All(limiter => limiter.IsValidAtStart(shiftTemplate, extenders));
        }

        private static void removeInvalidAtEnd(IList<IWorkShift> workingList, ICollection<IWorkShiftLimiter> limiters)
        {
            if(limiters.Count>0)
            {
                for (int i = workingList.Count - 1; i >= 0; i--)
                {
                    IWorkShift shift = workingList[i];
                    IVisualLayerCollection shiftProjection = shift.ProjectionService().CreateProjection();
                    foreach (IWorkShiftLimiter limiter in limiters)
                    {
                        if (!limiter.IsValidAtEnd(shiftProjection))
                        {
                            workingList.RemoveAt(i);
                            break;
                        }
                    }
                }                
            }
        }

        private static IList<IWorkShift> generateForOneExtender(IEnumerable<IWorkShift> workingList,
                                                                IWorkShiftExtender extender)
        {
            List<IWorkShift> returnCollection = new List<IWorkShift>();

            foreach (IWorkShift shift in workingList)
            {
                returnCollection.AddRange(extender.ReplaceWithNewShifts(shift));
            }
            return returnCollection;
        }
    }
}