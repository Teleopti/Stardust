using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
    {
        public void Remove(IScheduleDay part,ILayer<IActivity> layer)
        {
            if (part != null)
            {
							//no need to cast here when agentday exist and we have one list of layers...
	            var msActivityLayer = layer as IMainShiftLayer;
							if (msActivityLayer != null)
							{
								//Check for the layers in mainshift...
								foreach (var assignment in part.PersonAssignmentCollection())
								{
									if (assignment.RemoveLayer(msActivityLayer))
									{
										if (!assignment.MainShiftLayers.Any())
										{
											//rk - why is this here!?
											part.DeleteMainShift(part);
										}
										return;
									}
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
                            return;
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
                            return;
                        }
                    }
                }
            }
        }

        public void Remove(IScheduleDay part,ILayer<IAbsence> layer)
        {
            if (part != null)
            {
                foreach (IPersonAbsence personAbsence in part.PersonAbsenceCollection())
                {
                    if (personAbsence.Layer == layer)
                    {
                        part.Remove(personAbsence);
                        return;
                    }
                }
            }
        }
    }
}
