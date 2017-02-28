using System;
using System.ComponentModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// EventArgs for SchedulingServiceBase
    /// </summary>
    public abstract class SchedulingServiceBaseEventArgs : CancelEventArgs
    {
        private readonly IScheduleDay _schedulePart;
	    private readonly bool _isSuccessful;
	    private Action _cancelCallback;
		private static readonly Action DummyAction = ()=>{};

	    /// <summary>
	    /// Initializes a new instance of the <see cref="SchedulingServiceBaseEventArgs"/> class.
	    /// </summary>
	    /// <remarks>
	    /// Created by: micke
	    /// Created date: 2009-01-14
	    /// </remarks>
	    protected SchedulingServiceBaseEventArgs(IScheduleDay schedulePart, bool isSuccessful, Action cancelCallback = null)
	    {
		    _schedulePart = schedulePart;
		    _isSuccessful = isSuccessful;
		    _cancelCallback = cancelCallback ?? DummyAction;
	    }

	    /// <summary>
        /// Gets the schedule part.
        /// </summary>
        /// <value>The schedule part.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-14
        /// </remarks>
        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }

	    public bool IsSuccessful
	    {
		    get { return _isSuccessful; }
	    }

	    public Action CancelCallback
	    {
		    get { return _cancelCallback; }
	    }

	    public void AppendCancelAction(Action cancelAction)
	    {
		    var cancelCallback = _cancelCallback;
		    _cancelCallback = () =>
		    {
			    cancelAction();
			    cancelCallback();
		    };
	    }
    }
}
