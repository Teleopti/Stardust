
using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class WorkShiftFilterResult : IWorkShiftFilterResult
    {
        private readonly string _message;
        private readonly int _workShiftsBefore;
        private readonly int _workShiftsAfter;
    	private readonly Guid _key;

    	public WorkShiftFilterResult(string message, int workShiftsBefore, int workShiftsAfter)
			:this(message,workShiftsBefore,workShiftsAfter,Guid.NewGuid())
		{}
        
		public WorkShiftFilterResult(string message, int workShiftsBefore, int workShiftsAfter, Guid key)
		{
			_message = message;
			_workShiftsBefore = workShiftsBefore;
			_workShiftsAfter = workShiftsAfter;
			_key = key;
		}

        protected WorkShiftFilterResult(){}

        public string Message
        {
            get { return _message; }
        }

        public int     WorkShiftsBefore
        { get { return _workShiftsBefore; } }

        public int WorkShiftsAfter
        { get { return _workShiftsAfter; } }

		public Guid Key
		{
			get { return _key; }
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var ent = obj as IWorkShiftFilterResult;
			return ent != null && Equals(ent);
		}

		public virtual bool Equals(IWorkShiftFilterResult other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return (GetHashCode() == other.GetHashCode());
		}
    }
}
