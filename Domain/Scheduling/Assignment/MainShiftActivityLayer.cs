using System;
using Teleopti.Interfaces.Domain;
using AggregateException = Teleopti.Ccc.Domain.Common.AggregateException;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// MainshiftActivitylayer class
    /// </summary>
    public class MainShiftActivityLayer : ActivityLayer, IMainShiftActivityLayer
    {
		private IEntity _parent;
	    private Guid? _id;

	    public MainShiftActivityLayer(IActivity activity, DateTimePeriod period)
            : base(activity, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainShiftActivityLayer"/> class.
        /// </summary>
        protected MainShiftActivityLayer()
        {
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

		void IAggregateEntity.SetParent(IEntity parent)
		{
			SetParent(parent);
		}

		internal protected virtual void SetParent(IEntity parent)
		{
			Parent = parent;
		}


		public new virtual IEntity Parent
		{
			get { return _parent; }
			private set { _parent = value; }
		}
    }
}