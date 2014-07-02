using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public partial class SelectDatasourceScreen : UserControl, ILogonStep
	{
		private readonly ILogonView _logonView;
		private LogonModel _model;

		public SelectDatasourceScreen(ILogonView logonView, LogonModel model)
		{
		    _logonView = logonView;
		    _model = model;
            InitializeComponent();
            labelChooseDataSource.Text = Resources.PleaseChooseADatasource;
            tabPageWindowsDataSources.Text = Resources.WindowsLogon;
            tabPageApplicationDataSources.Text = Resources.ApplicationLogon;
			buttonLogOnCancel.Text = Resources.Cancel;
			buttonLogOnOK.Text = Resources.Ok;
			btnBack.Text = Resources.Back;
		}

		public void SetData()
		{
            var logonableWindowsDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Windows).ToList();
            var availableApplicationDataSources = _model.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application).ToList();

            listBoxApplicationDataSources.DataSource = availableApplicationDataSources;
            listBoxWindowsDataSources.DataSource = logonableWindowsDataSources;

		    Control itemToFocus;
			if (logonableWindowsDataSources.Count < 1)
			{
			    var idx = tabControlChooseDataSource.TabPages.IndexOf(tabPageWindowsDataSources);
                if(idx > -1)
                    tabControlChooseDataSource.TabPages.RemoveAt(idx);
				tabControlChooseDataSource.SelectedIndex = 0;
				itemToFocus  = listBoxApplicationDataSources;
			}
			else
			{
				itemToFocus = listBoxWindowsDataSources;
			}

            tabControlChooseDataSource.Visible = true;
		    itemToFocus.Focus();
		}

		public void GetData()
	    {
            _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxApplicationDataSources.SelectedItem;
            if (tabControlChooseDataSource.SelectedTab.Equals(tabPageWindowsDataSources))
                _model.SelectedDataSourceContainer = (IDataSourceContainer)listBoxWindowsDataSources.SelectedItem;
	    }

	    public void Release()
	    {
	        _model = null;
	        listBoxWindowsDataSources.DataSource = null;
	        listBoxApplicationDataSources.DataSource = null;
	    }

		public void SetBackButtonVisible(bool visible)
		{
			btnBack.Visible = visible;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			_logonView.HandleKeyPress(msg, keyData, listBoxWindowsDataSources.Focused || listBoxApplicationDataSources.Focused);
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void buttonLogOnOK_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnOkClick(sender, e);
		}

		private void buttonLogOnCancel_Click(object sender, System.EventArgs e)
		{
			_logonView.ButtonLogOnCancelClick(sender, e);		
		}

		private void btnBack_Click(object sender, System.EventArgs e)
		{
			_logonView.BtnBackClick(sender, e);
		}

		private void listBoxWindowsDataSources_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox listBox = (ListBox)sender;
			e.DrawBackground();
			Brush myBrush = Brushes.Black;

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				myBrush = Brushes.White;
				e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 153, 255)), e.Bounds);
			}

			else
			{
				e.Graphics.FillRectangle(Brushes.White, e.Bounds);

			}

			e.Graphics.DrawString(((DataSourceContainer)(listBox.Items[e.Index])).DataSourceName, e.Font, myBrush, e.Bounds);
			e.DrawFocusRectangle();
		}

		private void listBoxApplicationDataSources_DrawItem(object sender, DrawItemEventArgs e)
		{
			ListBox listBox = (ListBox)sender;
			e.DrawBackground();
			Brush myBrush = Brushes.Black;

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				myBrush = Brushes.White;
				e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 153, 255)), e.Bounds);
			}

			else
			{
				e.Graphics.FillRectangle(Brushes.White, e.Bounds);

			}

			e.Graphics.DrawString(((DataSourceContainer)(listBox.Items[e.Index])).DataSourceName, e.Font, myBrush, e.Bounds);
			e.DrawFocusRectangle();
		}
	}
}
