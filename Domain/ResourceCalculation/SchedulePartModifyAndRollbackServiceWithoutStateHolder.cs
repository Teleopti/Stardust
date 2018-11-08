﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SchedulePartModifyAndRollbackServiceWithoutStateHolder
    {
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private readonly IScheduleTagSetter _scheduleTagSetter;
        private readonly FixedCapacityStack<IScheduleDay> _rollbackStack = new FixedCapacityStack<IScheduleDay>(5000);

        public SchedulePartModifyAndRollbackServiceWithoutStateHolder(IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTagSetter scheduleTagSetter)
        {
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _scheduleTagSetter = scheduleTagSetter;
        }

	    public bool ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
	    {
		    var partToSave = schedulePart.ReFetch();
		    var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter, newBusinessRuleCollection);
		    _rollbackStack.Push(partToSave);
		    if (!responses.Any()) return true;

		    rollbackLast(NewBusinessRuleCollection.Minimum());
		    return false;
	    }
		
		public bool ModifyStrictly(IEnumerable<IScheduleDay> scheduleParts, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			var originalStackSize = _rollbackStack.Count;

			foreach (var schedulePart in scheduleParts)
			{
				var partToSave = schedulePart.ReFetch();
				var responses = modifyWithNoValidation(schedulePart, ScheduleModifier.Scheduler, scheduleTagSetter, newBusinessRuleCollection);
				
				_rollbackStack.Push(partToSave);
				if (responses.Any())
				{
					while(_rollbackStack.Count != originalStackSize)
						rollbackLast(NewBusinessRuleCollection.Minimum());

					return false;
				}
			}
			return true;
		}
		
	    public void RollbackMinimumChecks()
	    {
			while (_rollbackStack.Count > 0)
			{
				rollbackLast(NewBusinessRuleCollection.Minimum());
			}
		}

        private void rollbackLast(INewBusinessRuleCollection businessRuleCollection)
        {
            modifyWithNoValidation(_rollbackStack.Pop(), ScheduleModifier.UndoRedo, _scheduleTagSetter, businessRuleCollection);
        }

	    public void ClearModificationCollection()
        {
            _rollbackStack.Clear();
        }

		private IEnumerable<IBusinessRuleResponse> modifyWithNoValidation(IScheduleDay schedulePart, ScheduleModifier modifier, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			return schedulePart.Owner.Modify(modifier, schedulePart, newBusinessRuleCollection, _scheduleDayChangeCallback, scheduleTagSetter);
		}
    }
}
