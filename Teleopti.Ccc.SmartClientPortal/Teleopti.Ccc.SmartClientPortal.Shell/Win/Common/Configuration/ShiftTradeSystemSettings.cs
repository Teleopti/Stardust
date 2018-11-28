using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ShiftTradeSystemSettings : BaseUserControl, ISettingPage
	{
		private ShiftTradeSettings _shiftTradeSettings;
		private SFGridColumnGridHelper<ShiftTradeBusinessRuleConfigView> _gridColumnHelper;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;

		private static readonly Dictionary<RequestHandleOption, ShiftTradeRequestHandleOptionView> _shiftTradeRequestHandleOptionViews
			= new Dictionary<RequestHandleOption, ShiftTradeRequestHandleOptionView>()
			{
				{RequestHandleOption.AutoDeny, new ShiftTradeRequestHandleOptionView(RequestHandleOption.AutoDeny, Resources.Deny)},
				{RequestHandleOption.Pending, new ShiftTradeRequestHandleOptionView(RequestHandleOption.Pending, Resources.SendToAdministrator)}
			};

		private IList<ShiftTradeBusinessRuleConfigView> _businessRuleConfigViews;

		public ShiftTradeSystemSettings(IBusinessRuleConfigProvider businessRuleConfigProvider)
		{
			_businessRuleConfigProvider = businessRuleConfigProvider;
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

			shifTradeBusinessRuleSubHeader.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			lblShiftTradeBusinessRuleSettingHeader.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
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

			configBusinessRuleSettingGrid();
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
			syncRuleConfigs();

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
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

		public bool CheckPermission()
		{
			return PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests);
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

		private void syncRuleConfigs()
		{
			for (var i = 1; i < businessRuleSettingGrid.RowCount; i++)
			{
				var ruleName = businessRuleSettingGrid[i, 1].CellValue.ToString();
				var businessRuleConfig = _shiftTradeSettings.BusinessRuleConfigs.FirstOrDefault(b => b.FriendlyName == ruleName);
				if (businessRuleConfig == null) continue;
				businessRuleConfig.Enabled = bool.Parse(businessRuleSettingGrid[i, 2].CellValue.ToString());
				if (businessRuleConfig.Enabled)
				{
					businessRuleConfig.HandleOptionOnFailed =
						((ShiftTradeRequestHandleOptionView)businessRuleSettingGrid[i, 3].CellValue).RequestHandleOption;
				}
				else
				{
					businessRuleConfig.HandleOptionOnFailed = null;
				}
			}
		}

		private void configBusinessRuleSettingGrid()
		{
			var handleOptionColumn =
				new SFGridDropDownColumn<ShiftTradeBusinessRuleConfigView, ShiftTradeRequestHandleOptionView>("HandleOptionOnFailed",
					Resources.WhenValidationFails, _shiftTradeRequestHandleOptionViews.Values.ToList(), "Description", typeof(ShiftTradeRequestHandleOptionView));
			var gridColumns = new List<SFGridColumnBase<ShiftTradeBusinessRuleConfigView>>
			{
				new SFGridRowHeaderColumn<ShiftTradeBusinessRuleConfigView>(string.Empty),
				new SFGridReadOnlyTextColumn<ShiftTradeBusinessRuleConfigView>("Name", 300, Resources.Name),
				new SFGridCheckBoxColumn<ShiftTradeBusinessRuleConfigView>("Enabled", Resources.Enabled),
				handleOptionColumn
			};

			_businessRuleConfigViews = getShiftTradeBusinessRuleConfigViews().ToList();
			businessRuleSettingGrid.RowCount = _businessRuleConfigViews.Count;
			businessRuleSettingGrid.Height = businessRuleSettingGrid.RowCount * 25;
			businessRuleSettingGrid.CheckBoxClick += businessRuleSettingGridCheckBoxClick;
			businessRuleSettingGrid.QueryCellInfo += businessRuleSettingGridQueryCellInfo;

			_gridColumnHelper = new SFGridColumnGridHelper<ShiftTradeBusinessRuleConfigView>(businessRuleSettingGrid,
							   new ReadOnlyCollection<SFGridColumnBase<ShiftTradeBusinessRuleConfigView>>(gridColumns),
							   _businessRuleConfigViews, false)
			{ AllowExtendedCopyPaste = true };
		}

		private void businessRuleSettingGridQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.ColIndex != 3)
				return;

			var ruleName = businessRuleSettingGrid[e.RowIndex, 1].CellValue.ToString();
			var ruleConfig = _businessRuleConfigViews.FirstOrDefault(c => c.Name == ruleName);
			if (ruleConfig == null)
				return;

			e.Style.Enabled = ruleConfig.Enabled;
		}

		private void businessRuleSettingGridCheckBoxClick(object sender, GridCellClickEventArgs e)
		{
			if (e.ColIndex != 2)
				return;

			var oldValue = false;
			if (!bool.TryParse(businessRuleSettingGrid[e.RowIndex, e.ColIndex].CellValue.ToString(), out oldValue))
				return;

			var ruleName = businessRuleSettingGrid[e.RowIndex, 1].CellValue.ToString();
			var ruleConfig = _businessRuleConfigViews.FirstOrDefault(c => c.Name == ruleName);
			if (ruleConfig == null)
				return;

			if (oldValue)
			{
				ruleConfig.HandleOptionOnFailed = null;
			}
			else
			{
				var defaultBusinessRuleConfigs = getDefaultBusinessRuleConfigs();
				var defaultHandleOption = defaultBusinessRuleConfigs.FirstOrDefault(f => f.FriendlyName == ruleName)?
					.HandleOptionOnFailed.GetValueOrDefault(RequestHandleOption.Pending);
				ruleConfig.HandleOptionOnFailed = _shiftTradeRequestHandleOptionViews[defaultHandleOption.Value];
			}
			businessRuleSettingGrid.RefreshRange(GridRangeInfo.Cell(e.RowIndex, e.ColIndex + 1));
		}

		private IEnumerable<ShiftTradeBusinessRuleConfigView> getShiftTradeBusinessRuleConfigViews()
		{
			loadBusinessRuleConfigs();

			var businessRuleConfigViews = _shiftTradeSettings.BusinessRuleConfigs.Select
				(b =>
					new ShiftTradeBusinessRuleConfigView(b)
					{
						HandleOptionOnFailed = b.Enabled ? _shiftTradeRequestHandleOptionViews[b.HandleOptionOnFailed.Value] : null
					});
			return businessRuleConfigViews;
		}

		private void loadBusinessRuleConfigs()
		{
			var defaultBusinessRuleConfigs = getDefaultBusinessRuleConfigs();

			if (_shiftTradeSettings.BusinessRuleConfigs == null)
				_shiftTradeSettings.BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { };

			var businessRuleConfigList = _shiftTradeSettings.BusinessRuleConfigs.ToList();

			foreach (var existingConfig in businessRuleConfigList)
			{
				var businessRuleType = existingConfig.BusinessRuleType;
				if (defaultBusinessRuleConfigs.All(c => c.BusinessRuleType != businessRuleType)) continue;

				var defaultConfig = defaultBusinessRuleConfigs.First(c => c.BusinessRuleType == businessRuleType);
				defaultConfig.Enabled = existingConfig.Enabled;
				defaultConfig.HandleOptionOnFailed = existingConfig.HandleOptionOnFailed;
			}

			_shiftTradeSettings.BusinessRuleConfigs = defaultBusinessRuleConfigs.ToArray();
		}

		private void chkEnableMaxSeats_CheckedChanged(object sender, EventArgs e)
		{
			_shiftTradeSettings.MaxSeatsValidationEnabled = chkEnableMaxSeats.Checked;
			checkIntervalCheckBoxEnabled();
		}

		private void cmbSegmentSizeMaxSeatValidation_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (cmbSegmentSizeMaxSeatValidation.SelectedItem == null) return;

			var selectedIntervalLengthItem = (IntervalLengthItem)cmbSegmentSizeMaxSeatValidation.SelectedItem;
			_shiftTradeSettings.MaxSeatsValidationSegmentLength = selectedIntervalLengthItem.Minutes;
		}

		private void buttonResetRule_Click(object sender, EventArgs e)
		{
			_shiftTradeSettings.BusinessRuleConfigs = null;
			_businessRuleConfigViews = getShiftTradeBusinessRuleConfigViews().ToList();
			_gridColumnHelper.SetSourceList(_businessRuleConfigViews);
		}

		private IEnumerable<ShiftTradeBusinessRuleConfig> getDefaultBusinessRuleConfigs()
		{
			return _businessRuleConfigProvider.GetDefaultConfigForShiftTradeRequest()
				.Select(s => (ShiftTradeBusinessRuleConfig) s).ToList();
		}
	}
}