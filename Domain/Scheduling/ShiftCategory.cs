using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{


    /// <summary>
    /// Represents a shift category, a grouping of shifts
    /// </summary>
    public class ShiftCategory : VersionedAggregateRootWithBusinessUnit, IShiftCategory, IDeleteTag, ICloneableEntity<ShiftCategory>
    {
        #region Fields

        private Description _description;
        private Color _displayColor;
        private DayOfWeekIntDictionary _dayOfWeekJusticeValues = new DayOfWeekIntDictionary();
        private bool _isDeleted;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a shiftCategory with a name
        /// </summary>
        /// <param name="name"></param>
        public ShiftCategory(string name) :this()
        {
            _description = new Description(name);
        }

        /// <summary>
        /// For NHibernate
        /// </summary>
        protected ShiftCategory()
        {
            
        }

        #endregion



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

        public virtual IDictionary<DayOfWeek, int> DayOfWeekJusticeValues
        {
            get { return _dayOfWeekJusticeValues; }
        }

        public virtual int MaxOfJusticeValues()
        {
            int result = 0;
            foreach (KeyValuePair<DayOfWeek, int> pair in _dayOfWeekJusticeValues)
            {
                if (pair.Value > result)
                    result = pair.Value;
            }
            return result;
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }


        public virtual void ReinitializeDayOfWeekDictionary()
        {
            _dayOfWeekJusticeValues = new DayOfWeekIntDictionary();
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