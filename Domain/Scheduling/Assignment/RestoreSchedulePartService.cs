﻿using Teleopti.Interfaces.Domain;

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
            _destination.Clear<IPersonAbsence>();
            _destination.Clear<IPersonDayOff>();

            foreach (var personAssingment in _source.PersonAssignmentCollection())
            {
                _destination.Add(personAssingment.NoneEntityClone());
            }

            foreach (var personAbsence in _source.PersonAbsenceCollection())
            {
                _destination.Add(personAbsence.NoneEntityClone());
            }

            foreach (var personDayOff in _source.PersonDayOffCollection())
            {
                _destination.Add(personDayOff.NoneEntityClone());
            }

            _rollbackService.Modify(_destination);   
        }
    }
}
