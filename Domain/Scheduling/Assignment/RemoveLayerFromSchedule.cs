using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
    {
			//no need to loop assignments multiple times - however, there will soon only be ONE assignment....
        public void Remove(IScheduleDay part,ILayer<IActivity> layer)
        {
            if (part != null)
            {
							//no need to cast here when agentday exist and we have one list of layers...
	            var msActivityLayer = layer as IMainShiftLayer;
							if (msActivityLayer != null)
							{
								//Check for the layers in mainshift...
								foreach (var assignment in part.PersonAssignmentCollectionDoNotUse())
								{
									if (assignment.RemoveLayer(msActivityLayer))
									{
										if (!assignment.MainLayers().Any())
										{
											//rk - why is this here!?
											part.DeleteMainShift(part);
										}
										return;
									}
								}
							}
                
                //Check for the layer in personalShift
                foreach (var assignment in part.PersonAssignmentCollectionDoNotUse())
                {
	                foreach (var personalLayer in assignment.PersonalLayers())
	                {
		                if (layer.Equals(personalLayer))
		                {
			                assignment.RemoveLayer(personalLayer);
			                return;
		                }
	                }
                }

                //Check for the layer in overtime
                foreach (var assignment in part.PersonAssignmentCollectionDoNotUse())
                {
	                foreach (var overtimeLayer in assignment.OvertimeLayers())
	                {
		                if (layer.Equals(overtimeLayer))
		                {
			                assignment.RemoveLayer(overtimeLayer);
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
