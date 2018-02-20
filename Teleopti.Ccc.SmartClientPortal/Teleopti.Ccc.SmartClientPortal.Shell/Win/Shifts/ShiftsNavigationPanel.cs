using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public partial class ShiftsNavigationPanel : SmartClientPortal.Shell.Win.Common.BaseUserControl
    {
        private readonly ShiftsModule.IShiftsExplorerFactory _shiftsExplorerFactory;
	    private Form _mainWindow;
		private readonly IApplicationInsights _applicationInsights;

	    public ShiftsNavigationPanel(ShiftsModule.IShiftsExplorerFactory shiftsExplorerFactory, IApplicationInsights applicationInsights)
        {
            _shiftsExplorerFactory = shiftsExplorerFactory;
			_applicationInsights = applicationInsights;
			InitializeComponent();
            SetTexts();
        }

        private void open()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                _shiftsExplorerFactory.Create(_mainWindow);
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.Shifts,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
            }

            Cursor.Current = Cursors.Default;
			_applicationInsights.TrackEvent("Opened shifts in Shifts Module.");
		}

        public void OpenShifts()
        {
            open();
        }

	    public void SetMainOwner(Form mainWindow)
	    {
		    _mainWindow = mainWindow;
	    }

		private void toolStripButton1_Click(object sender, System.EventArgs e)
		{
			open();
		}

		private void ShiftsNavigationPanel_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Enter)
				OpenShifts();
		}
    }
}