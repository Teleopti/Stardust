using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{

	public partial class MasterActivitiesControl : BaseUserControl, ISettingPage, IMasterActivityView
	{
		private MasterActivityPresenter _presenter;
		private bool _selecting;

		public MasterActivitiesControl()
		{
			InitializeComponent();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			IMasterActivityViewModel model = new MasterActivityViewModel(ActivityRepository.DONT_USE_CTOR(value), MasterActivityRepository.DONT_USE_CTOR(value));
			_presenter = new MasterActivityPresenter(this, model);
		}

		public void Persist()
		{}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{}

		public ViewType ViewType
		{
			get { return ViewType.MasterActivity; }
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void LoadControl()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			ColorButton.SelectedAsBackcolor = true;
			_presenter.LoadAllMasterActivities();
			_presenter.OnMasterActivitySelected((IMasterActivityModel)comboBoxMasterActivities.SelectedItem);
			hookEvents();
		}

		public void SaveChanges()
		{}

		public void Unload()
		{
			unHookEvents();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.MasterActivity;
		}

		public void OnShow()
		{
		}

		private void hookEvents()
		{
			comboBoxMasterActivities.SelectedIndexChanged += comboBoxMasterActivitiesSelectedIndexChanged;
			textBoxName.Leave += masterChanged;
			twoListSelector1.SelectedAdded += masterChanged;
			twoListSelector1.SelectedRemoved += masterChanged;
			ColorButton.BackColorChanged += masterChanged;
		}

		private void unHookEvents()
		{
			if (comboBoxMasterActivities == null)
				return;
			comboBoxMasterActivities.SelectedIndexChanged -= comboBoxMasterActivitiesSelectedIndexChanged;
			textBoxName.Leave -= masterChanged;
			twoListSelector1.SelectedAdded -= masterChanged;
			twoListSelector1.SelectedRemoved -= masterChanged;
			
			ColorButton.BackColorChanged -= masterChanged;
		}

		void masterChanged(object sender, EventArgs e)
		{
			if (_selecting) return;

			_presenter.OnMasterActivityPropertyChanged(getSelected());
		}

		void comboBoxMasterActivitiesSelectedIndexChanged(object sender, EventArgs e)
		{
			_selecting = true;
			_presenter.OnMasterActivitySelected(getSelected());
			_selecting = false;
		}

		private IMasterActivityModel getSelected()
		{
			return (IMasterActivityModel)comboBoxMasterActivities.SelectedItem;
		}

		public void SetUpdateInfo(string infoText)
		{
			autoLabelInfoAboutChanges.Text = infoText;
		}

		public void SelectMaster(IMasterActivityModel masterActivityModel)
		{
			if (masterActivityModel == null) return;
			comboBoxMasterActivities.SelectedValue = masterActivityModel.Name;
		}

		public bool ConfirmDelete()
		{
			if (getSelected() == null) return false;
			var text = string.Format(
					CurrentCulture,
					Resources.AreYouSureYouWantToDeleteMasterActivity,
					getSelected().Name
					);

				var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

				var response = ViewBase.ShowConfirmationMessage(text, caption);
				return response == DialogResult.Yes;
		}

		public void LoadComboWithMasterActivities(IList<IMasterActivityModel> masterActivities)
		{
			var tmpList = new List<IMasterActivityModel>(masterActivities);
			comboBoxMasterActivities.DataSource = null;
			comboBoxMasterActivities.DisplayMember = "Name";
			comboBoxMasterActivities.ValueMember = "Name";
			comboBoxMasterActivities.DataSource = tmpList;
		}

		public void LoadTwoList(IList<IActivityModel> allActivities, IList<IActivityModel> selectedActivities)
		{
			twoListSelector1.Initiate(allActivities, selectedActivities, "Name", Resources.NotSelectedActivities, Resources.SelectedActivities);
		}

		public Color Color
		{
			get { return ColorButton.SelectedColor; }
			set { ColorButton.SelectedColor = value; }
		}

		public string LongName
		{
			get { return textBoxName.Text; }
			set { textBoxName.Text = value; }
		}

		public IList<IActivityModel> Activities
		{
			get { return twoListSelector1.GetSelected<IActivityModel>(); }
		}


		private void buttonAddClick(object sender, EventArgs e)
		{
			unHookEvents();
			_presenter.OnAddNew();
			hookEvents();
		}

		private void buttonDeleteClick(object sender, EventArgs e)
		{
			_presenter.OnDeleteMasterActivity(getSelected());
		}

		private void button1Click(object sender, EventArgs e)
		{
			ColorButton.PerformClick();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

            tableLayoutPanel.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

	}
}
