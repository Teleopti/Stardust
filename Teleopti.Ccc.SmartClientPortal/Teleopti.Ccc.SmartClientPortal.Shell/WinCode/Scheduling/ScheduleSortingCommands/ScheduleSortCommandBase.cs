using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class ScheduleSortCommandBase
    {
        private readonly ISchedulerStateHolder _schedulerState;
        private IList<IVisualLayerCollection> _projections = new List<IVisualLayerCollection>();
        private List<IVisualLayerCollection> _absence = new List<IVisualLayerCollection>();
        private List<IPerson> _dayOff = new List<IPerson>();
        private List<IPerson> _empty = new List<IPerson>();

        protected ScheduleSortCommandBase(ISchedulerStateHolder schedulerState)
        {
            _schedulerState = schedulerState;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        protected IList<IVisualLayerCollection> Projections
        {
            get { return _projections; }
            set { _projections = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        protected List<IVisualLayerCollection> Absence
        {
            get { return _absence; }
            set { _absence = value; }
        }


        protected void CreateLists(DateOnly dateOnly)
        {
            _projections = new List<IVisualLayerCollection>();
            _absence = new List<IVisualLayerCollection>();
            _dayOff = new List<IPerson>();
            _empty = new List<IPerson>();

            foreach (var person in _schedulerState.FilteredAgentsDictionary.Values)
            {
                IScheduleDay scheduleDay = _schedulerState.Schedules[person].ScheduledDay(dateOnly);
                SchedulePartView significant = scheduleDay.SignificantPart();
                if (significant == SchedulePartView.MainShift || significant == SchedulePartView.Overtime)
                {
                    Projections.Add(scheduleDay.ProjectionService().CreateProjection());
                    continue;
                }

                if (significant == SchedulePartView.FullDayAbsence)
                {
                    Absence.Add(scheduleDay.ProjectionService().CreateProjection());
                    continue;
                }

                if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                {
                    _dayOff.Add(person);
                    continue;
                }

                _empty.Add(person);
            }
        }

        protected void MergeLists()
        {
			_schedulerState.FilteredAgentsDictionary.Clear();
            foreach (var projection in Projections)
            {
                if (projection.Person.Id != null)
                    _schedulerState.FilteredAgentsDictionary.Add(projection.Person.Id.Value, projection.Person);
            }
            foreach (var projection in Absence)
            {
                if (projection.Person.Id != null)
                    _schedulerState.FilteredAgentsDictionary.Add(projection.Person.Id.Value, projection.Person);
            }
            foreach (var person in _dayOff)
            {
                if (person.Id != null)
                    _schedulerState.FilteredAgentsDictionary.Add(person.Id.Value, person);
            }
            foreach (var person in _empty)
            {
                if (person.Id != null)
                    _schedulerState.FilteredAgentsDictionary.Add(person.Id.Value, person);
            }
        }
    }
}