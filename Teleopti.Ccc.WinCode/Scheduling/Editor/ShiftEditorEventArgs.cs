using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Editor
{
    /// <summary>
    /// Event args for shift editor
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-19
    /// </remarks>
    public class ShiftEditorEventArgs : CancelEventArgs
    {
        private readonly IScheduleDay _schedulePart;
				private readonly ILayer<IPayload> _selectedLayer;

        public DateTimePeriod? Period { get; set; }

        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }


        /// <summary>
        /// Gets the selected layer.
        /// </summary>
        /// <value>The selected layer.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-06-09
        /// </remarks>
				public ILayer<IPayload> SelectedLayer
        {
            get { return _selectedLayer; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftEditorEventArgs"/> class.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-02    
        /// /// </remarks>
        public ShiftEditorEventArgs(IScheduleDay schedulePart)
        {
            _schedulePart = schedulePart;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftEditorEventArgs"/> class.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="selectedLayer">The selected layer.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-07-02    
        /// /// </remarks>
        public ShiftEditorEventArgs(IScheduleDay schedulePart, ILayer<IPayload> selectedLayer)
        {
            _schedulePart = schedulePart;
            _selectedLayer = selectedLayer;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftEditorEventArgs"/> class. 
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>

        /// <param name="period">The period. </param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-11-20
        /// </remarks>
        public ShiftEditorEventArgs(IScheduleDay schedulePart, DateTimePeriod period)
        {
            _schedulePart = schedulePart;
            Period = period;
        }
    }
}
