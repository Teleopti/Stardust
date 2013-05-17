using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersistedActivityLayer : ActivityLayer, IPersistedActivityLayer
	{
		private Guid? _id;
		private IEntity _parent;

		 public PersistedActivityLayer(IActivity activity, DateTimePeriod period)
			: base(activity,period)
		{}

		protected PersistedActivityLayer(){}

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

		 public override IEntity Parent
		{
			get { return _parent; }
		}

		 IAggregateRoot IAggregateEntity.Root()
		 {
			 return Root(_parent);
		 }

		 [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IAggregateEntity")]
		 protected virtual IAggregateRoot Root(IEntity parent)
		 {
			 var theParent = parent;
			 var root = theParent as IAggregateRoot;

			 while (root == null)
			 {
				 var internalParent = theParent as IAggregateEntity;
				 if (internalParent == null)
				 {
					 throw new AggregateException("[" + ToString() + "]:s parent is null or not of type IAggregateEntity");
				 }

				 theParent = internalParent.Parent;
				 root = theParent as IAggregateRoot;
			 }
			 return root;
		 }

		public override void SetParent(IEntity parent)
		 {
			 _parent = parent;
		 }
	}
}