using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
	{
		public void Remove(IScheduleDay part, ILayer<IActivity> layer)
		{
			if (part == null)
				return;
			var ass = part.PersonAssignment();
			if (ass == null)
				return;

			//no need to cast here when agentday exist and we have one list of layers...
			var msActivityLayer = layer as IMainShiftLayer;
			if (msActivityLayer != null)
			{
				if (ass.RemoveLayer(msActivityLayer))
				{
					if (!ass.MainLayers().Any())
					{
						//rk - why is this here!?
						part.DeleteMainShift(part);
					}
					return;
				}
			}

			//Check for the layer in personalShift
			foreach (var personalLayer in ass.PersonalLayers()
												.Where(personalLayer => layer.Equals(personalLayer)))
			{
				ass.RemoveLayer(personalLayer);
				return;
			}

			//Check for the layer in overtime
			foreach (var overtimeLayer in ass.OvertimeLayers()
												.Where(overtimeLayer => layer.Equals(overtimeLayer)))
			{
				ass.RemoveLayer(overtimeLayer);
				return;
			}
		}

		public void Remove(IScheduleDay part, ILayer<IAbsence> layer)
		{
			if (part == null) return;
			foreach (var personAbsence in part.PersonAbsenceCollection()
													.Where(personAbsence => personAbsence.Layer.Equals(layer)))
			{
				part.Remove(personAbsence);
				return;
			}
		}
	}
}
