using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.ConfigTool.Code.Gui.StartupConfiguration;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration
{
	public partial class StartupConfigurationView : Form, IStartupConfigurationView
	{
		private readonly StartupConfigurationPresenter _presenter;

		public StartupConfigurationView(IConfigurationHandler configurationHandler)
		{
			_presenter = new StartupConfigurationPresenter(this, new StartupConfigurationModel(configurationHandler));
			InitializeComponent();
		}

		public void LoadCultureList(ReadOnlyCollection<LookupIntegerItem> cultureList)
		{
			comboBoxCulture.ValueMember = "Id";
			comboBoxCulture.DisplayMember = "Text";
			comboBoxCulture.DataSource = cultureList;
		}

		public void SetDefaultCulture(LookupIntegerItem lookupIntegerItem)
		{
			comboBoxCulture.SelectedItem = lookupIntegerItem;
		}

		public void LoadIntervalLengthList(ReadOnlyCollection<LookupIntegerItem> intervalLengthList)
		{
			comboBoxIntervalLength.ValueMember = "Id";
			comboBoxIntervalLength.DisplayMember = "Text";
			comboBoxIntervalLength.DataSource = intervalLengthList;
		}

		public void SetDefaultIntervalLength(int intervalLengthMinutes)
		{
			comboBoxIntervalLength.SelectedValue = intervalLengthMinutes;
		}

		public void SetDefaultTimeZone(LookupStringItem timeZone)
		{
			comboBoxTimeZone.SelectedItem = timeZone;
		}

		public void LoadTimeZoneList(ReadOnlyCollection<LookupStringItem> timeZoneList)
		{
			comboBoxTimeZone.ValueMember = "Id";
			comboBoxTimeZone.DisplayMember = "Text";
			comboBoxTimeZone.DataSource = timeZoneList;
		}

		public void DisableIntervalLength()
		{
			comboBoxIntervalLength.Enabled = false;
		}

		public void DisableOkButton()
		{
			buttonOk.Enabled = false;
		}

		public void ShowErrorMessage(string message)
		{
			labelMessage.Text = message;
		}

		public object SelectedIntervalLengthValue
		{
			get { return comboBoxIntervalLength.SelectedValue; }
		}

		private void StartupConfigurationView_Load(object sender, EventArgs e)
		{
			_presenter.Initialize();
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			var culture = (int)comboBoxCulture.SelectedValue;
			var intervalLength = (int)comboBoxIntervalLength.SelectedValue;
			var timeZone = (string)comboBoxTimeZone.SelectedValue;
			_presenter.Save(culture, intervalLength, timeZone);
		}
	}
}
