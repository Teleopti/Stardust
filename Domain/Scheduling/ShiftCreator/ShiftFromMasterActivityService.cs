using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class ShiftFromMasterActivityService : IShiftFromMasterActivityService
    {
        public IList<IWorkShift> Generate(IWorkShift workShift)
        {
            if (HasMasterActivity(workShift))
            {
                IList<IWorkShift> workShifts = new List<IWorkShift>();
                using(PerformanceOutput.ForOperation("Creating shifts from master activity"))
                CreateShiftsFromShift(workShift, workShifts);

                return workShifts; 
            }
           
            return null;    
        }

        private static void CreateShiftsFromShift(IWorkShift workShift, IList<IWorkShift> workShifts)
        {
            IVisualLayerCollection visualLayers = workShift.ProjectionService().CreateProjection();

            foreach (VisualLayer layer in visualLayers)
            {
                IMasterActivity masterActivity = layer.Payload as MasterActivity;

                if (masterActivity != null)
                {
                    foreach (IActivity activity in masterActivity.ActivityCollection)
                    {
                        if (activity.IsDeleted == false && activity.InContractTime && activity.RequiresSkill)
                        {
                            IWorkShift newWorkShift = new WorkShift(workShift.ShiftCategory);

                            foreach (ActivityLayer a in workShift.LayerCollection)
                            {
                                IMasterActivity ma = a.Payload as MasterActivity;

                                if (ma == null)
                                {
                                    newWorkShift.LayerCollection.Add(a);
                                }
                            }

                            WorkShiftActivityLayer activityLayer = new WorkShiftActivityLayer(activity, layer.Period);
                            newWorkShift.LayerCollection.Add(activityLayer);

                            foreach (VisualLayer l in visualLayers)
                            {
                                IMasterActivity m = l.Payload as MasterActivity;

                                if (l != layer && m != null)
                                {
                                    IActivity act = l.Payload as Activity;
                                    WorkShiftActivityLayer acl = new WorkShiftActivityLayer(act, l.Period);
                                    newWorkShift.LayerCollection.Add(acl);
                                }
                            }

                            if (HasMasterActivity(newWorkShift))
                                CreateShiftsFromShift(newWorkShift, workShifts);
                            else
                                workShifts.Add(newWorkShift);
                        }
                    }

                    break;
                }
            }
        }

        private static bool HasMasterActivity(IWorkShift workShift)
        {
            foreach (ILayer<IActivity> activity in workShift.LayerCollection)
            {
                IMasterActivity masterActivity = activity.Payload as MasterActivity;

                if (masterActivity != null)
                    return true;
            }

            return false;
        }
    }
}
