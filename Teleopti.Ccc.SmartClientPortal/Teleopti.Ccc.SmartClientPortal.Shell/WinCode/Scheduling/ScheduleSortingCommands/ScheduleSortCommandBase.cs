using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class ScheduleSortCommandBase
    {
        private readonly SchedulingScreenState _schedulerState;
        private IList<Tuple<IVisualLayerCollection, IPerson>> _projections = new List<Tuple<IVisualLayerCollection, IPerson>>();
        private List<Tuple<IVisualLayerCollection, IPerson>> _absence = new List<Tuple<IVisualLayerCollection, IPerson>>();
        private List<IPerson> _dayOff = new List<IPerson>();
        private List<IPerson> _empty = new List<IPerson>();

        protected ScheduleSortCommandBase(SchedulingScreenState schedulerState)
        {
            _schedulerState = schedulerState;
        }

        protected IList<Tuple<IVisualLayerCollection, IPerson>> Projections
        {
            get { return _projections; }
            set { _projections = value; }
        }

        protected List<Tuple<IVisualLayerCollection, IPerson>> Absence
        {
            get { return _absence; }
            set { _absence = value; }
        }


        protected void CreateLists(DateOnly dateOnly)
        {
            _projections = new List<Tuple<IVisualLayerCollection, IPerson>>();
            _absence = new List<Tuple<IVisualLayerCollection, IPerson>>();
            _dayOff = new List<IPerson>();
            _empty = new List<IPerson>();

            foreach (var person in _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Values)
            {
                IScheduleDay scheduleDay = _schedulerState.SchedulerStateHolder.Schedules[person].ScheduledDay(dateOnly);
                SchedulePartView significant = scheduleDay.SignificantPart();
                if (significant == SchedulePartView.MainShift || significant == SchedulePartView.Overtime)
                {
                    Projections.Add(new Tuple<IVisualLayerCollection, IPerson>(scheduleDay.ProjectionService().CreateProjection(), scheduleDay.Person));
                    continue;
                }

                if (significant == SchedulePartView.FullDayAbsence)
                {
                    Absence.Add(new Tuple<IVisualLayerCollection, IPerson>(scheduleDay.ProjectionService().CreateProjection(), scheduleDay.Person));
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
			_schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Clear();
            foreach (var projection in Projections)
            {
                if (projection.Item2.Id != null)
                    _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Add(projection.Item2.Id.Value, projection.Item2);
            }
            foreach (var projection in Absence)
            {
                if (projection.Item2.Id != null)
                    _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Add(projection.Item2.Id.Value, projection.Item2);
            }
            foreach (var person in _dayOff)
            {
                if (person.Id != null)
                    _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Add(person.Id.Value, person);
            }
            foreach (var person in _empty)
            {
                if (person.Id != null)
                    _schedulerState.SchedulerStateHolder.FilteredAgentsDictionary.Add(person.Id.Value, person);
            }
        }
    }
}