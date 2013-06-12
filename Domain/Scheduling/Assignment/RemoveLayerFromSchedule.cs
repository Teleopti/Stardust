using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    
    public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
    {

        public IScheduleDay Remove(IScheduleDay part,ILayer<IActivity> layer)
        {
            if (part != null)
            {
                //Check for the layers in mainshift...
                foreach (var assignment in part.PersonAssignmentCollection())
                {
#pragma warning disable 612,618
	                var mainShift = assignment.ToMainShift();
#pragma warning restore 612,618
					if (mainShift == null)
                        continue;
					if (mainShift.LayerCollection.Contains(layer))
                    {
						mainShift.LayerCollection.Remove(layer);
						if (mainShift.LayerCollection.Count == 0)
                            part.DeleteMainShift(part);
						else
							assignment.SetMainShift(mainShift);
                        return part;
                    }
                }

                //Check for the layer in personalShift
                foreach (var assignment in part.PersonAssignmentCollection())
                {
                    foreach (IPersonalShift shift in assignment.PersonalShiftCollection)
                    {
                        if (shift.LayerCollection.Contains(layer))
                        {
                            shift.LayerCollection.Remove(layer);
                            if (shift.LayerCollection.Count == 0)
                            {
                                assignment.RemovePersonalShift(shift);
                            }
                            return part;
                        }
                    }
                }

                //Check for the layer in overtime
                foreach (var assignment in part.PersonAssignmentCollection())
                {
                    //Check personalshifts and mainshift where its possible to have an activitylayer:
                    foreach (IOvertimeShift shift in assignment.OvertimeShiftCollection)
                    {
                        if (shift.LayerCollection.Contains(layer))
                        {
                            shift.LayerCollection.Remove(layer);
                            if (shift.LayerCollection.Count == 0)
                            {
                                assignment.RemoveOvertimeShift(shift);
                            }
                            return part;
                        }
                    }
                }
            }
            return part;
        }

        public IScheduleDay Remove(IScheduleDay part,ILayer<IAbsence> layer)
        {
            if (part != null)
            {
                foreach (IPersonAbsence personAbsence in part.PersonAbsenceCollection())
                {
                    if (personAbsence.Layer == layer)
                    {
                        part.Remove(personAbsence);
                        return part;
                    }
                }
            }
            return part;
        }

    }
}
