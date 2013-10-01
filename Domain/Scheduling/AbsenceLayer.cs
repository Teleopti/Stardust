using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using AggregateException = System.AggregateException;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Layer class containing Absences
    /// </summary>
	public class AbsenceLayer : Layer<IAbsence>, IAbsenceLayer
    {
		private IEntity _parent;
		private Guid? _id;
        
        protected AbsenceLayer()
        {
        }

        public AbsenceLayer(IAbsence abs, DateTimePeriod period) : base(abs, period)
        {
            InParameter.EnsureNoSecondsInPeriod(period);
        }

        public override DateTimePeriod Period
        {
            get
            {
                return base.Period;
            }
            set
            {
                InParameter.EnsureNoSecondsInPeriod(value);
                base.Period = value;
            }
        }

		public virtual bool Equals(IEntity other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;
			if (!other.Id.HasValue || !Id.HasValue)
				return false;

			return (Id.Value == other.Id.Value);
		}

		public virtual Guid? Id
		{
			get { return _id; }
		}

		public virtual void SetId(Guid? newId)
		{
			if (newId.HasValue)
			{
				_id = newId;
			}
			else
			{
				ClearId();
			}
		}

		public virtual void ClearId()
		{
			_id = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IAggregateEntity")]
		protected virtual IAggregateRoot Root()
		{
			var parent = Parent;
			var root = parent as IAggregateRoot;

			while (root == null)
			{
				var internalParent = parent as IAggregateEntity;
				if (internalParent == null)
				{
					throw new AggregateException("[" + ToString() + "]:s parent is null or not of type IAggregateEntity");
				}

				parent = internalParent.Parent;
				root = parent as IAggregateRoot;
			}
			return root;
		}

		IAggregateRoot IAggregateEntity.Root()
		{
			return Root();
		}

		public void SetParent(IEntity parent)
		{
			Parent = parent;
		}

		public virtual IEntity Parent
		{
			get { return _parent; }
			private set { _parent = value; }
		}
    }
}