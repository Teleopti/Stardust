using System;
using System.ComponentModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public class ResourceOptimizerProgressEventArgs : CancelEventArgs
    {
        private readonly string _message;
	    private readonly int _screenRefreshRate;
	    private readonly Action _cancelAction;
	    private readonly double _value;
        private readonly double _delta;
	    private static readonly Action DummyAction = () => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceOptimizerProgressEventArgs"/> class.
        /// </summary>
        public ResourceOptimizerProgressEventArgs(double value, double delta, string message, int screenRefreshRate, Action cancelAction = null)
        {
            _value = value;
            _delta = delta;
            _message = message;
	        _screenRefreshRate = screenRefreshRate;
	        _cancelAction = cancelAction ?? DummyAction;
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

	    public Action CancelAction
	    {
		    get { return _cancelAction; }
	    }

	    public int ScreenRefreshRate
	    {
		    get { return _screenRefreshRate; }
	    }
    }
}
