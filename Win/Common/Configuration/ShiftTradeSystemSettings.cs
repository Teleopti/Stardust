using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class ShiftTradeSystemSettings : BaseUserControl, ISettingPage
	{
		private ShiftTradeSettings _shiftTradeSettings;

		public ShiftTradeSystemSettings()
		{
			InitializeComponent();
		}

		public void SetUnitOfWork (IUnitOfWork value)
		{
		}

		public void LoadFromExternalModule (SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		
		public void InitializeDialogControl()
		{
			SetTexts();
			setColors();
		}


		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			lblShiftTradeMaxSeatsSettings.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}


		private void initIntervalLengthComboBox(int defaultLength)
		{
			var intervalLengths = new List<IntervalLengthItem>
												{
													new IntervalLengthItem(10),
													new IntervalLengthItem(15),
													new IntervalLengthItem(30),
													new IntervalLengthItem(60)
												};

			cmbSegmentSizeMaxSeatValidation.DataSource = intervalLengths;
			cmbSegmentSizeMaxSeatValidation.DisplayMember = "Text";
			IntervalLengthItem selectedIntervalLengthItem = intervalLengths.FirstOrDefault(length => length.Minutes == defaultLength);
			cmbSegmentSizeMaxSeatValidation.SelectedItem = selectedIntervalLengthItem;
		}

		public void LoadControl()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_shiftTradeSettings = new GlobalSettingDataRepository(uow).FindValueByKey(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings());
			}

			chkEnableMaxSeats.Checked = _shiftTradeSettings.MaxSeatsValidationEnabled;
			initIntervalLengthComboBox(_shiftTradeSettings.MaxSeatsValidationSegmentLength);

			checkIntervalCheckBoxEnabled();

		}

		private void checkIntervalCheckBoxEnabled()
		{
			cmbSegmentSizeMaxSeatValidation.Enabled = chkEnableMaxSeats.Checked;
		}

		public void SaveChanges()
		{
			Persist();
		}


		public void Persist()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_shiftTradeSettings = new GlobalSettingDataRepository(uow).PersistSettingValue(_shiftTradeSettings).GetValue(new ShiftTradeSettings());
			
				uow.PersistAll();
			}
		}


		public void Unload()
		{
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.SystemSettings);
		}

		public string TreeNode()
		{
			return Resources.ShiftTradeRequestSettings;
		}

		public void OnShow()
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.SystemSetting; }
		}

		private void chkEnableMaxSeats_CheckedChanged(object sender, EventArgs e)
		{
			_shiftTradeSettings.MaxSeatsValidationEnabled = chkEnableMaxSeats.Checked;
			checkIntervalCheckBoxEnabled();
		}


		private void cmbSegmentSizeMaxSeatValidation_SelectedIndexChanged (object sender, EventArgs e)
		{
			var selectedIntervalLengthItem = (IntervalLengthItem)cmbSegmentSizeMaxSeatValidation.SelectedItem;
			_shiftTradeSettings.MaxSeatsValidationSegmentLength = selectedIntervalLengthItem.Minutes;
		}
	}
}
