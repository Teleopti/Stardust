
namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduleReportDialogGraphicalModel
	{
		private bool _team;
		private bool _individual = true;
		private bool _oneFileForSelected = true;
	    private bool _showPublicNote = false;
		private bool _sortOnAgentName;
		private bool _sortOnStartTime = true;
		private bool _sortOnEndTime;

		public bool Team
		{
			get { return _team; }
			set { _team = value; }
		}

		public bool Individual
		{
			get { return _individual; }
			set { _individual = value; }
		}

		public bool OneFileForSelected
		{
			get { return _oneFileForSelected; }
			set { _oneFileForSelected = value; }
		}

	    public bool ShowPublicNote
	    {
            get { return _showPublicNote; }
            set { _showPublicNote = value; }
	    }

		public bool SortOnAgentName
		{
			get { return _sortOnAgentName; }
			set { _sortOnAgentName = value; }
		}

		public bool SortOnStartTime
		{
			get { return _sortOnStartTime; }
			set { _sortOnStartTime = value; }
		}

		public bool SortOnEndTime
		{
			get { return _sortOnEndTime; }
			set { _sortOnEndTime = value; }
		}
	}
}
