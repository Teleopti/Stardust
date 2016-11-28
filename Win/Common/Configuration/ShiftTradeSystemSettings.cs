using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration.Columns;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class ShiftTradeSystemSettings : BaseUserControl, ISettingPage
	{
		private ShiftTradeSettings _shiftTradeSettings;
		private SFGridColumnGridHelper<ShiftTradeBusinessRuleConfigView> _gridColumnHelper;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;
		private readonly IToggleManager _toggleManager;
		private static readonly Dictionary<RequestHandleOption, ShiftTradeRequestHandleOptionView> _shiftTradeRequestHandleOptionViews
			= new Dictionary<RequestHandleOption, ShiftTradeRequestHandleOptionView>()
			{
				{RequestHandleOption.AutoDeny, new ShiftTradeRequestHandleOptionView(RequestHandleOption.AutoDeny, Resources.Deny)},
				{
					RequestHandleOption.Pending,
					new ShiftTradeRequestHandleOptionView(RequestHandleOption.Pending, Resources.SendToAdministrator)
				}
			};

		public ShiftTradeSystemSettings(IToggleManager toggleManager, IBusinessRuleConfigProvider businessRuleConfigProvider)
		{
			_toggleManager = toggleManager;
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

		private void configBusinessRuleSettingGrid()
		{
			if (!_toggleManager.IsEnabled(Toggles.Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770))
			{
				shiftTradeBusinessRuleSettingPanel.Visible = false;
				return;
			}

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

			var businessRuleConfigViews = getShiftTradeBusinessRuleConfigViews().ToList();
			businessRuleSettingGrid.RowCount = businessRuleConfigViews.Count;
			businessRuleSettingGrid.Height = businessRuleSettingGrid.RowCount * 20;

			_gridColumnHelper = new SFGridColumnGridHelper<ShiftTradeBusinessRuleConfigView>(businessRuleSettingGrid,
							   new ReadOnlyCollection<SFGridColumnBase<ShiftTradeBusinessRuleConfigView>>(gridColumns),
							   businessRuleConfigViews, false)
			{ AllowExtendedCopyPaste = true };
		}

		private IEnumerable<ShiftTradeBusinessRuleConfigView> getShiftTradeBusinessRuleConfigViews()
		{
			if (_shiftTradeSettings.BusinessRuleConfigs == null)
			{
				_shiftTradeSettings.BusinessRuleConfigs =
					_businessRuleConfigProvider.GetDefaultConfigForShiftTradeRequest().Select(s=>(ShiftTradeBusinessRuleConfig)s).ToArray();
			}

			var businessRuleConfigViews = _shiftTradeSettings.BusinessRuleConfigs.Select
				(b =>
					new ShiftTradeBusinessRuleConfigView(b)
					{
						HandleOptionOnFailed = _shiftTradeRequestHandleOptionViews[b.HandleOptionOnFailed]
					});
			return businessRuleConfigViews;
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
			var shiftTradeBusinessRuleConfigViews = getShiftTradeBusinessRuleConfigViews();
			_gridColumnHelper.SetSourceList(shiftTradeBusinessRuleConfigViews.ToList());
		}
	}
}