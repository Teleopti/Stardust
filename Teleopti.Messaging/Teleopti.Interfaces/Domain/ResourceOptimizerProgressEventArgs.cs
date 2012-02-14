﻿using System.ComponentModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used by ResourceReoptimizer
    /// </summary>
    public class ResourceOptimizerProgressEventArgs : CancelEventArgs
    {
        private readonly string _message;
        private readonly double _value;
        private readonly double _delta;
        private readonly IPerson _person;
        private bool _userCancel;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOptimizerProgressEventArgs"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="value">The value.</param>
        /// <param name="delta">The delta.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-26
        /// </remarks>
        public ResourceOptimizerProgressEventArgs(IPerson person, double value, double delta)
            : this(person, value, delta, string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOptimizerProgressEventArgs"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="value">The value.</param>
        /// <param name="delta">The delta.</param>
        /// <param name="message">The message.</param>
        public ResourceOptimizerProgressEventArgs(IPerson person, double value, double delta, string message)
        {
            _value = value;
            _person = person;
            _delta = delta;
            _message = message;
        }


        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-26
        /// </remarks>
        public double Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-27
        /// </remarks>
        public IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-27
        /// </remarks>
        public double Delta
        {
            get { return _delta; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether user cancel.
        /// </summary>
        /// <value><c>true</c> if user cancel; otherwise, <c>false</c>.</value>
        public bool UserCancel
        {
            get { return _userCancel; }
            set { _userCancel = value; }
        }
    }
}
