using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class RestoreSchedulePartService
    {

				public void Restore(IScheduleDay current, IScheduleDay historical)
        {
	        var currAss = current.PersonAssignment();
					current.Clear<IPersonAssignment>();
					current.Clear<IPersonAbsence>();


					//needs some love and refactoring///
	        var historyAssignment = historical.PersonAssignment();
					if (historyAssignment != null)
					{
						//wants deep cloning of layers etc
						var historyClone = historyAssignment.NoneEntityClone();
						if (currAss != null)
						{
							//need to set the old id, making update instead of insert
							historyClone.SetId(currAss.Id);
							historyClone.SetVersion(currAss.Version.Value);
						}
						current.Add(historyClone);
					}
					////////////////////////////////////


					foreach (var personAbsence in historical.PersonAbsenceCollection())
            {
                current.Add(personAbsence.NoneEntityClone());
            }
        }
    }
}
