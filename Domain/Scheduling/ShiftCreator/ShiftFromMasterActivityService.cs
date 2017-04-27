using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class ShiftFromMasterActivityService : IShiftFromMasterActivityService
    {
        public IList<IWorkShift> ExpandWorkShiftsWithMasterActivity(IWorkShift workShift)
        {
            if (hasMasterActivity(workShift))
            {
	            using (PerformanceOutput.ForOperation("Creating shifts from master activity"))
	            {
		            return createShiftsFromShift(workShift);
	            }
            }
           
            return new[]{workShift};
        }

	    protected virtual DateTimePeriod FirstLayerPeriod(DateTimePeriod layerPeriod, DateTimePeriod workShiftPeriod)
	    {
		    return layerPeriod;
	    }

        private IList<IWorkShift> createShiftsFromShift(IWorkShift workShift)
        {
			var result = new List<IWorkShift>();
            IVisualLayerCollection visualLayers = workShift.ProjectionService().CreateProjection();

            foreach (var layer in visualLayers)
            {
                IMasterActivity masterActivity = layer.Payload as MasterActivity;
                if (masterActivity != null)
                {
                    foreach (var activity in masterActivity.ActivityCollection)
                    {
                        if (activity.IsDeleted == false && activity.InContractTime && activity.RequiresSkill)
                        {
                            IWorkShift newWorkShift = new WorkShift(workShift.ShiftCategory);

                            foreach (var a in workShift.LayerCollection)
                            {
                                IMasterActivity ma = a.Payload as MasterActivity;

                                if (ma == null)
                                {
                                    newWorkShift.LayerCollection.Add(a);
                                }
                            }

                            WorkShiftActivityLayer activityLayer = new WorkShiftActivityLayer(activity, FirstLayerPeriod(layer.Period, workShift.LayerCollection.Period().GetValueOrDefault()));
                            newWorkShift.LayerCollection.Add(activityLayer);

                            foreach (var l in visualLayers)
                            {
                                IMasterActivity m = l.Payload as MasterActivity;

                                if (l != layer && m != null)
                                {
                                    IActivity act = l.Payload as Activity;
                                    var acl = new WorkShiftActivityLayer(act, l.Period);
                                    newWorkShift.LayerCollection.Add(acl);
                                }
                            }

                            if (hasMasterActivity(newWorkShift))
                                result.AddRange(createShiftsFromShift(newWorkShift));
                            else
                                result.Add(newWorkShift);
                        }
                    }

                    break;
                }
            }
	        return result;
        }

        private static bool hasMasterActivity(IWorkShift workShift)
        {
			return workShift.LayerCollection.Any(l => l.Payload is IMasterActivity);
        }
    }
}
