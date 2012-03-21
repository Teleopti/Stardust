using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Win.Common.Configuration
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
			IMasterActivityViewModel model = new MasterActivityViewModel(new ActivityRepository(value), new MasterActivityRepository(value));
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
		{}

		public void LoadControl()
		{
			SetTexts();
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			ColorButton.SelectedAsBackcolor = true;
			_presenter.LoadAllMasterActivities();
			_presenter.OnMasterActivitySelected((IMasterActivityModel)comboBoxMasterActivities.SelectedItem);
			HookEvents();
		}

		public void SaveChanges()
		{}

		public void Unload()
		{
			UnHookEvents();
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

		private void HookEvents()
		{
			comboBoxMasterActivities.SelectedIndexChanged += ComboBoxMasterActivitiesSelectedIndexChanged;
			textBoxName.Leave += MasterChanged;
			twoListSelector1.SelectedAdded += MasterChanged;
			twoListSelector1.SelectedRemoved += MasterChanged;
			ColorButton.BackColorChanged += MasterChanged;
		}

		private void UnHookEvents()
		{
			if (comboBoxMasterActivities == null)
				return;
			comboBoxMasterActivities.SelectedIndexChanged -= ComboBoxMasterActivitiesSelectedIndexChanged;
			textBoxName.Leave -= MasterChanged;
			twoListSelector1.SelectedAdded -= MasterChanged;
			twoListSelector1.SelectedRemoved -= MasterChanged;
			
			ColorButton.BackColorChanged -= MasterChanged;
		}

		void MasterChanged(object sender, EventArgs e)
		{
			if (_selecting) return;

			_presenter.OnMasterActivityPropertyChanged(GetSelected());
		}

		void ComboBoxMasterActivitiesSelectedIndexChanged(object sender, EventArgs e)
		{
			_selecting = true;
			_presenter.OnMasterActivitySelected(GetSelected());
			_selecting = false;
		}

		private IMasterActivityModel GetSelected()
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
			if (GetSelected() == null) return false;
			var text = string.Format(
					CurrentCulture,
					Resources.AreYouSureYouWantToDeleteMasterActivity,
					GetSelected().Name
					);

				var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

				var response = ViewBase.ShowConfirmationMessage(text, caption);
				return response == DialogResult.Yes;
		}

		public void LoadComboWithMasterActivities(IList<IMasterActivityModel> masterActivities)
		{
			comboBoxMasterActivities.DataSource = null;
			comboBoxMasterActivities.DisplayMember = "Name";
			comboBoxMasterActivities.ValueMember = "Name";
			comboBoxMasterActivities.DataSource = masterActivities;
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


		private void ButtonAddClick(object sender, EventArgs e)
		{
			UnHookEvents();
			_presenter.OnAddNew();
			HookEvents();
		}

		private void ButtonDeleteClick(object sender, EventArgs e)
		{
			_presenter.OnDeleteMasterActivity(GetSelected());
		}

		private void Button1Click(object sender, EventArgs e)
		{
			ColorButton.PerformClick();
		}



	}
}
