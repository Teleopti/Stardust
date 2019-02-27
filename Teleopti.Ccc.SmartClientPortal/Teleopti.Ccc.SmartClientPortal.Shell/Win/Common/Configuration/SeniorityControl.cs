using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class SeniorityControl : BaseUserControl, ISettingPage, ISeniorityControlView
	{
		private IUnitOfWork _unitOfWork;
		private SeniorityControlPresenter _presenter;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public SeniorityControl()
		{
			InitializeComponent();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
		}

		public void Persist()
		{
			_presenter.Persist();
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
			panelShiftCategory.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelWorkDays.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelShiftCategory.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelWorkingDays.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		public void LoadControl()
		{
			if (Disposing) return;
			var seniorityWorkDayRankingsRepository = SeniorityWorkDayRanksRepository.DONT_USE_CTOR(_unitOfWork);
			var shiftCategoryRepository = new ShiftCategoryRepository(_unitOfWork);
			_presenter = new SeniorityControlPresenter(this, seniorityWorkDayRankingsRepository, shiftCategoryRepository);
			_presenter.Initialize();

			RefreshListBoxWorkingDays(0);
			RefreshListBoxShiftCategoryRank(0);
		}

		public void SaveChanges()
		{
			Persist();
		}

		public void Unload()
		{
			if(_presenter != null) _presenter.Unload();	
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.Seniority;
		}

		public void OnShow()
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.Seniority; }
		}

		public void RefreshListBoxWorkingDays(int selectedIndex)
		{
			var workDays = _presenter.SeniorityWorkDays();
			listBoxWorkingDays.DataSource = workDays;
			listBoxWorkingDays.DisplayMember = "DayOfWeekName";
			listBoxWorkingDays.ValueMember = "DayOfWeek";
			if(!workDays.IsEmpty()) listBoxWorkingDays.SelectedIndex = selectedIndex;
		}

		public void RefreshListBoxShiftCategoryRank(int selectedIndex)
		{
			var ranks = _presenter.SeniorityShiftCategoryRanks();
			listBoxShiftCatgory.DataSource = ranks;
			listBoxShiftCatgory.DisplayMember = "Text";
			listBoxShiftCatgory.ValueMember = "ShiftCategory";
			if(!ranks.IsEmpty()) listBoxShiftCatgory.SelectedIndex = selectedIndex;	
		}

		public void SetChangedInfo(ISeniorityWorkDayRanks workDayRanks)
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);

			var changed = _localizer.UpdatedByText(workDayRanks, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		private void buttonTopWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveTopWorkDay(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonUpWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveUpWorkDay(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonDownWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveDownWorkDay(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonBottomWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveBottomWorkDay(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonTopShiftCategoryClick(object sender, EventArgs e)
		{
			_presenter.MoveTopShiftCategory(listBoxShiftCatgory.SelectedIndex);
		}

		private void buttonUpShiftCategoryClick(object sender, EventArgs e)
		{
			_presenter.MoveUpShiftCategory(listBoxShiftCatgory.SelectedIndex);
		}

		private void buttonDownShiftCategoryClick(object sender, EventArgs e)
		{
			_presenter.MoveDownShiftCategory(listBoxShiftCatgory.SelectedIndex);
		}

		private void buttonBottomShiftCategoryClick(object sender, EventArgs e)
		{
			_presenter.MoveBottomShiftCategory(listBoxShiftCatgory.SelectedIndex);
		}
	}
}
