using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class SeniorityControl : BaseUserControl, ISettingPage, ISeniorityControlView
	{
		private IUnitOfWork _unitOfWork;
		private SeniorityControlPresenter _presenter;

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
			var seniorityWorkDayRankingsRepository = new SeniorityWorkDayRanksRepository(_unitOfWork);
			_presenter = new SeniorityControlPresenter(this, seniorityWorkDayRankingsRepository);
			_presenter.Initialize();

			RefreshListBoxWorkingDays(0);
		}

		public void SaveChanges()
		{
			Persist();
		}

		public void Unload()
		{
			_presenter.Unload();	
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
			listBoxWorkingDays.DataSource = _presenter.SeniorityWorkDays();
			listBoxWorkingDays.DisplayMember = "DayOfWeekName";
			listBoxWorkingDays.ValueMember = "DayOfWeek";
			listBoxWorkingDays.SelectedIndex = selectedIndex;
		}

		private void buttonTopWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveTop(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonUpWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveUp(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonDownWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveDown(listBoxWorkingDays.SelectedIndex);
		}

		private void buttonBottomWorkDayClick(object sender, EventArgs e)
		{
			_presenter.MoveBottom(listBoxWorkingDays.SelectedIndex);
		}
	}
}
