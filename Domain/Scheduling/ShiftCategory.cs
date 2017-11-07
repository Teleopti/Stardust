using System.Drawing;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{


    /// <summary>
    /// Represents a shift category, a grouping of shifts
    /// </summary>
    public class ShiftCategory : VersionedAggregateRootWithBusinessUnit, IShiftCategory, IDeleteTag, ICloneableEntity<ShiftCategory>, IAggregateRootWithEvents
	{
        #region Fields

        private Description _description;
        private Color _displayColor;
        private bool _isDeleted;
	    private int? _rank;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a shiftCategory with a name
        /// </summary>
        /// <param name="name"></param>
        public ShiftCategory(string name)
        {
            _description = new Description(name);
        }

		public ShiftCategory() : this("_")
        {
            
        }

		#endregion

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
		
		#region Properties

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-26
		/// </remarks>
		public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets the color of a ShiftCategory
        /// </summary>
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

	    public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual object Clone()
        {
            return EntityClone();
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public virtual ShiftCategory NoneEntityClone()
        {
            var clone = (ShiftCategory)MemberwiseClone();
            clone.SetId(null);

            return clone;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public virtual ShiftCategory EntityClone()
        {
            return (ShiftCategory)MemberwiseClone();
        }
       
    }
}