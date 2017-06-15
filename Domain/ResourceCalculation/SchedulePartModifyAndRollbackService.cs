using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	//TODO: remove stateholder dep
    public class SchedulePartModifyAndRollbackService : ISchedulePartModifyAndRollbackService
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IScheduleTagSetter _scheduleTagSetter;
        private readonly FixedCapacityStack<IScheduleDay> _rollbackStack = new FixedCapacityStack<IScheduleDay>(5000);

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
			var partToSave = schedulePart.ReFetch();
			modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, _scheduleTagSetter, newBusinessRuleCollection);
			_rollbackStack.Push(partToSave);
		}

		public void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter)
        {
			var partToSave = schedulePart.ReFetch();
			modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter);
            _rollbackStack.Push(partToSave);
        }

		public bool ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			var partToSave = schedulePart.ReFetch();
			var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter, newBusinessRuleCollection);
			_rollbackStack.Push(partToSave);
			if (responses.Any())
			{
				rollbackLast(NewBusinessRuleCollection.AllForScheduling(_stateHolder));
				return false;
			}

			return true;
		}

	    public bool ModifyStrictlyRollbackWithoutValidation(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
	    {
		    var partToSave = schedulePart.ReFetch();
		    var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter, newBusinessRuleCollection);
		    _rollbackStack.Push(partToSave);
		    if (!responses.Any()) return true;

		    rollbackLast(NewBusinessRuleCollection.Minimum());
		    return false;
	    }


		//TEMP check if we could change old one instead
	    public void RollbackMinimumChecks()
	    {
			while (_rollbackStack.Count > 0)
			{
				rollbackLast(NewBusinessRuleCollection.Minimum());
			}
		}
	
		//change number of business rules here?
        public void Rollback()
        {
            using (PerformanceOutput.ForOperation("SchedulePartModifyAndRollbackService performed a rollback"))
            {
                while (_rollbackStack.Count > 0)
                {
                    rollbackLast(NewBusinessRuleCollection.AllForScheduling(_stateHolder));
                }
            }
        }

        private void rollbackLast(INewBusinessRuleCollection businessRuleCollection)
        {
            modifyWithNoValidation(_rollbackStack.Pop(), ScheduleModifier.UndoRedo, _scheduleTagSetter, businessRuleCollection);
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
        }

		public IEnumerable<IBusinessRuleResponse> ModifyParts(IEnumerable<IScheduleDay> scheduleDays)
    	{
						var scheduleParts = scheduleDays as IList<IScheduleDay> ?? scheduleDays.ToList();
			foreach (var scheduleDay in scheduleParts)
			{
				var partToSave = scheduleDay.ReFetch();
				_rollbackStack.Push(partToSave);
			}

			return _stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleParts,
			                                              NewBusinessRuleCollection.AllForScheduling(_stateHolder),
			                                              _scheduleDayChangeCallback, _scheduleTagSetter);
    	}

		private void modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter)
        {
           _stateHolder.Schedules.Modify(modifier, schedulePart, NewBusinessRuleCollection.AllForScheduling(_stateHolder), _scheduleDayChangeCallback, scheduleTagSetter);
        }

		private IEnumerable<IBusinessRuleResponse> modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			return schedulePart.Owner.Modify(modifier, schedulePart, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);
		}
    }
}
