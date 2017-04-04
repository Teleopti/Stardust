using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class RemoveLayerFromSchedule : IRemoveLayerFromSchedule
	{
		public void Remove(IScheduleDay part, ShiftLayer layer)
		{
			var ass = part?.PersonAssignment();
			ass?.RemoveActivity(layer);
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
