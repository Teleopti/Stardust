using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
    public partial class ShiftsNavigationPanel : SmartClientPortal.Shell.Win.Common.BaseUserControl
    {
        private readonly ShiftsModule.IShiftsExplorerFactory _shiftsExplorerFactory;
	    private Form _mainWindow;

	    public ShiftsNavigationPanel(ShiftsModule.IShiftsExplorerFactory shiftsExplorerFactory)
        {
            _shiftsExplorerFactory = shiftsExplorerFactory;
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