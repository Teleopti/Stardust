using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
	{
		public void Remove(IScheduleDay part, IShiftLayer layer)
		{
			if (part == null)
				return;
			var ass = part.PersonAssignment();
			if (ass == null)
				return;
			ass.RemoveActivity(layer);
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
