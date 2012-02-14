using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SchedulePartModifyAndRollbackService : ISchedulePartModifyAndRollbackService
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IScheduleTagSetter _scheduleTagSetter;
        private readonly FixedCapacityStack<IScheduleDay> _rollbackStack = new FixedCapacityStack<IScheduleDay>(5000);
        private readonly FixedCapacityStack<IScheduleDay> _modificationStack = new FixedCapacityStack<IScheduleDay>(5000);

        public SchedulePartModifyAndRollbackService(ISchedulingResultStateHolder stateHolder, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTagSetter scheduleTagSetter)
        {
            _stateHolder = stateHolder;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _scheduleTagSetter = scheduleTagSetter;
        }

        public void Modify(IScheduleDay schedulePart)
		{
			Modify(schedulePart, _scheduleTagSetter);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter)
        {
            IScheduleRange range = _stateHolder.Schedules[schedulePart.Person];
            IScheduleDay partToSave = range.ReFetch(schedulePart);
            modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter);
            _rollbackStack.Push(partToSave);
            _modificationStack.Push(schedulePart);
        }

        public void Rollback()
        {
            using (PerformanceOutput.ForOperation("SchedulePartModifyAndRollbackService performed a rollback"))
            {
                while (_rollbackStack.Count > 0)
                {
                    RollbackLast();
                }
            }
        }

        public void RollbackLast()
        {
            modifyWithNoValidation(_rollbackStack.Pop(), ScheduleModifier.UndoRedo, _scheduleTagSetter);
        }

        public int StackLength
        {
            get { return _rollbackStack.Count; }
        }

        public ReadOnlyCollection<IScheduleDay> ModificationCollection
        {
            get
            {
                return new ReadOnlyCollection<IScheduleDay>(_rollbackStack.ToList());
            }
        }

        public void ClearModificationCollection()
        {
            _rollbackStack.Clear();
            _modificationStack.Clear();
        }

        private void modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter)
        {
            _stateHolder.Schedules.Modify(modifier, schedulePart, NewBusinessRuleCollection.AllForScheduling(_stateHolder), _scheduleDayChangeCallback, scheduleTagSetter);
        }
    }
}
