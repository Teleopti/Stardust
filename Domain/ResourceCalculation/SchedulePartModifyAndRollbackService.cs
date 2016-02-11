using System.Collections.Generic;
using System.Linq;
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

		public void Modify(IScheduleDay schedulePart, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			IScheduleRange range = _stateHolder.Schedules[schedulePart.Person];
			IScheduleDay partToSave = range.ReFetch(schedulePart);
			modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, _scheduleTagSetter, newBusinessRuleCollection);
			_rollbackStack.Push(partToSave);
			_modificationStack.Push(schedulePart);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter)
        {
            IScheduleRange range = _stateHolder.Schedules[schedulePart.Person];
            IScheduleDay partToSave = range.ReFetch(schedulePart);
            var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter);
            _rollbackStack.Push(partToSave);
            _modificationStack.Push(schedulePart);
        	return responses;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			IScheduleRange range = _stateHolder.Schedules[schedulePart.Person];
			IScheduleDay partToSave = range.ReFetch(schedulePart);
			var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter, newBusinessRuleCollection);
			_rollbackStack.Push(partToSave);
			_modificationStack.Push(schedulePart);
			if (responses.Any())
				rollbackLast();
		}
	
        public void Rollback()
        {
            using (PerformanceOutput.ForOperation("SchedulePartModifyAndRollbackService performed a rollback"))
            {
                while (_rollbackStack.Count > 0)
                {
                    rollbackLast();
                }
            }
        }

        private void rollbackLast()
        {
            modifyWithNoValidation(_rollbackStack.Pop(), ScheduleModifier.UndoRedo, _scheduleTagSetter);
        }

        public IEnumerable<IScheduleDay> ModificationCollection
        {
            get
            {
                return _rollbackStack;
            }
        }

        public void ClearModificationCollection()
        {
            _rollbackStack.Clear();
            _modificationStack.Clear();
        }

		public IEnumerable<IBusinessRuleResponse> ModifyParts(IEnumerable<IScheduleDay> scheduleDays)
    	{
						var scheduleParts = scheduleDays as IList<IScheduleDay> ?? scheduleDays.ToList();
			foreach (var scheduleDay in scheduleParts)
			{
				var partToSave = scheduleDay.ReFetch();
				_rollbackStack.Push(partToSave);
				_modificationStack.Push(scheduleDay);
			}

			return _stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleParts,
			                                              NewBusinessRuleCollection.AllForScheduling(_stateHolder),
			                                              _scheduleDayChangeCallback, _scheduleTagSetter);
    	}

		private IEnumerable<IBusinessRuleResponse> modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter)
        {
           return _stateHolder.Schedules.Modify(modifier, schedulePart, NewBusinessRuleCollection.AllForScheduling(_stateHolder), _scheduleDayChangeCallback, scheduleTagSetter);
        }

		private IEnumerable<IBusinessRuleResponse> modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			return _stateHolder.Schedules.Modify(modifier, schedulePart, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);
		}
    }
}
