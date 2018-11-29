using System.ComponentModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    public class ShiftEditorEventArgs : CancelEventArgs
    {
        private readonly IScheduleDay _schedulePart;
		private readonly ILayer<IPayload> _selectedLayer;

        public DateTimePeriod? Period { get; set; }

        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }
		
		public ILayer<IPayload> SelectedLayer
        {
            get { return _selectedLayer; }
        }

		public ShiftEditorEventArgs(IScheduleDay schedulePart)
		{
			_schedulePart = schedulePart;
		}

        public ShiftEditorEventArgs(IScheduleDay schedulePart, ILayer<IPayload> selectedLayer)
        {
            _schedulePart = schedulePart;
            _selectedLayer = selectedLayer;
        }

		public ShiftEditorEventArgs(IScheduleDay schedulePart, DateTimePeriod period, ILayer<IPayload> selectedLayer)
        {
            _schedulePart = schedulePart;
            Period = period;
	        _selectedLayer = selectedLayer;
        }

		public ShiftEditorEventArgs(IScheduleDay schedulePart, DateTimePeriod period)
		{
			_schedulePart = schedulePart;
			Period = period;
		}
    }
}
