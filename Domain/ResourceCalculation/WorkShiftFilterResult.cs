﻿
using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class WorkShiftFilterResult
    {
	    public WorkShiftFilterResult(string message, int workShiftsBefore, int workShiftsAfter)
			:this(message,workShiftsBefore,workShiftsAfter,Guid.NewGuid())
		{}
        
		public WorkShiftFilterResult(string message, int workShiftsBefore, int workShiftsAfter, Guid key)
		{
			Message = message;
			WorkShiftsBefore = workShiftsBefore;
			WorkShiftsAfter = workShiftsAfter;
			Key = key;
		}
		
        public string Message { get; }

	    public int WorkShiftsBefore { get; }

	    public int WorkShiftsAfter { get; }

	    public Guid Key { get; }

	    public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var ent = obj as WorkShiftFilterResult;
			return ent != null && Equals(ent);
		}

		public virtual bool Equals(WorkShiftFilterResult other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return Key == other.Key;
		}
    }
}
