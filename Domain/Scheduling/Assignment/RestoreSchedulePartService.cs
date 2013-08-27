using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public interface IRestoreSchedulePartService
    {
        void Restore();
    }

    public class RestoreSchedulePartService : IRestoreSchedulePartService
    {
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IScheduleDay _destination;
        private readonly IScheduleDay _source;

        public RestoreSchedulePartService(ISchedulePartModifyAndRollbackService rollbackService, IScheduleDay destination, IScheduleDay source)
        {
            _rollbackService = rollbackService;
            _destination = destination;
            _source = source;
        }

        public void Restore()
        {
            _destination.Clear<IPersonAssignment>();
        	IList<IPersonAbsence> toDelete = _destination.PersonAbsenceCollection();
        	foreach (var personAbsence in toDelete)
        	{
        		_destination.Remove(personAbsence);
        	}

	        var personAssignment = _source.PersonAssignment();
					if (personAssignment != null)
					{
						_destination.Add(personAssignment.NoneEntityClone());
					}

            foreach (var personAbsence in _source.PersonAbsenceCollection())
            {
                _destination.Add(personAbsence.NoneEntityClone());
            }

            _rollbackService.Modify(_destination);   
        }
    }
}
