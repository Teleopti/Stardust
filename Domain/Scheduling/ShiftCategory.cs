using System.Drawing;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ShiftCategory : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IShiftCategory, IDeleteTag, ICloneableEntity<ShiftCategory>
	{
		private Description _description;
        private Color _displayColor;
        private bool _isDeleted;
	    private int? _rank;

        public ShiftCategory(string name)
        {
            _description = new Description(name);
        }

		public ShiftCategory() : this("_")
        {
        }

		public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
					AddEvent(new ShiftCategoryChangedEvent
					{
						ShiftCategoryId = Id.GetValueOrDefault()
					});
					break;
				case DomainUpdateType.Delete:
					AddEvent(new ShiftCategoryDeletedEvent
					{
						ShiftCategoryId = Id.GetValueOrDefault()
					});
					break;
			}
		}

		public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set { _displayColor = value.IsEmpty ? Color.Red : value; }
        }

	    public virtual int? Rank
	    {
			get { return _rank; }
			set { _rank = value; }
	    }

	    public virtual bool IsDeleted => _isDeleted;

		public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual ShiftCategory NoneEntityClone()
        {
            var clone = (ShiftCategory)MemberwiseClone();
            clone.SetId(null);

            return clone;
        }

        public virtual ShiftCategory EntityClone()
        {
            return (ShiftCategory)MemberwiseClone();
        }
    }
}