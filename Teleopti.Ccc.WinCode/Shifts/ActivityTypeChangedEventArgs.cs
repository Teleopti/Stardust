﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts
{
    public enum ActivityType
    {
        /// <summary>
        /// Auto position
        /// </summary>
        AutoPosition,
        /// <summary>
        /// Absolute position
        /// </summary>
        AbsolutePosition,
    }

    /// <summary>
    /// Event argument for the activity type changed event
    /// </summary>
    public class ActivityTypeChangedEventArgs : EventArgs
    {
        private readonly ActivityType _type;
        private readonly IActivityViewModel _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTypeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="item">The item.</param>
        public ActivityTypeChangedEventArgs(ActivityType type, IActivityViewModel item) 
        {
            _type = type;
            _item = item;
        }
		
        public ActivityType ActivityType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>The item.</value>
        public IActivityViewModel Item
        {
            get { return _item; }
        }
    }
}
