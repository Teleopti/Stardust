﻿using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Arguments for modified schedulepart event
    /// </summary>
    public class ModifyEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyEventArgs"/> class.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <param name="person">The person.</param>
        /// <param name="period">The period.</param>
        public ModifyEventArgs(ScheduleModifier modifier, IPerson person, DateTimePeriod period, IScheduleDay modifiedPart)
        {
            Modifier = modifier;
            ModifiedPerson = person;
            ModifiedPeriod = period;
	        ModifiedPart = modifiedPart;
        }

        /// <summary>
        /// Gets or sets the modifier.
        /// </summary>
        /// <value>The modifier.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-11
        /// </remarks>
        public ScheduleModifier Modifier { get; private set; }

        /// <summary>
        /// Person that is modified
        /// </summary>
        /// <value></value>
        public IPerson ModifiedPerson { get; private set; }

        /// <summary>
        /// Period that is modified
        /// </summary>
        /// <value></value>
        public DateTimePeriod ModifiedPeriod { get; private set; }

		public IScheduleDay ModifiedPart
		{
			get;
			private set;
		}
    }
}