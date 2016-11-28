using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
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

			initIntervalLengthComboBox(_shiftTradeSettings.MaxSeatsValidationSegmentLength);

			configBusinessRuleSettingGrid();
		}

		public void SaveChanges()
		{
			Persist();
		}

		public void Persist()
		{
			var shiftTradeMaxSeatsSpecificationRuleConfig = _shiftTradeSettings.BusinessRuleConfigs.FirstOrDefault(
				b => b.BusinessRuleType == typeof(ShiftTradeMaxSeatsSpecification).FullName);
			if (shiftTradeMaxSeatsSpecificationRuleConfig != null)
			{
				_shiftTradeSettings.MaxSeatsValidationEnabled = shiftTradeMaxSeatsSpecificationRuleConfig.Enabled;
			}

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
			var gridColumns = new List<SFGridColumnBase<ShiftTradeBusinessRuleConfigView>>
			{
				new SFGridRowHeaderColumn<ShiftTradeBusinessRuleConfigView>(string.Empty),
				new SFGridReadOnlyTextColumn<ShiftTradeBusinessRuleConfigView>("Name", 300, Resources.Name),
				new SFGridCheckBoxColumn<ShiftTradeBusinessRuleConfigView>("Enabled", Resources.Enabled)
			};

			if (isBusinessRuleConfigurable())
			{
				var handleOptionColumn =
					new SFGridDropDownColumn<ShiftTradeBusinessRuleConfigView, ShiftTradeRequestHandleOptionView>(
						"HandleOptionOnFailed",
						Resources.WhenValidationFails, _shiftTradeRequestHandleOptionViews.Values.ToList(), "Description",
						typeof(ShiftTradeRequestHandleOptionView));
				gridColumns.Add(handleOptionColumn);
			}

			var businessRuleConfigViews = getShiftTradeBusinessRuleConfigViews().ToList();
			businessRuleSettingGrid.RowCount = businessRuleConfigViews.Count;
			businessRuleSettingGrid.Height = (businessRuleSettingGrid.RowCount + 1) * 20;

			_gridColumnHelper = new SFGridColumnGridHelper<ShiftTradeBusinessRuleConfigView>(businessRuleSettingGrid,
							   new ReadOnlyCollection<SFGridColumnBase<ShiftTradeBusinessRuleConfigView>>(gridColumns),
							   businessRuleConfigViews, false)
			{ AllowExtendedCopyPaste = true };
		}

		private IEnumerable<ShiftTradeBusinessRuleConfigView> getShiftTradeBusinessRuleConfigViews()
		{
			loadBusinessRuleConfigs();
	
			var businessRuleConfigViews = _shiftTradeSettings.BusinessRuleConfigs.Select
				(b =>
					new ShiftTradeBusinessRuleConfigView(b)
					{
						HandleOptionOnFailed = _shiftTradeRequestHandleOptionViews[b.HandleOptionOnFailed]
					});
			return businessRuleConfigViews;
		}

		private void loadBusinessRuleConfigs()
		{
			var defaultConfigs = _businessRuleConfigProvider.GetDefaultConfigForShiftTradeRequest()
				.Select(s => (ShiftTradeBusinessRuleConfig) s);
			if (!isBusinessRuleConfigurable())
			{
				defaultConfigs = defaultConfigs.Where(c => c.BusinessRuleType == typeof(ShiftTradeMaxSeatsSpecification).FullName)
					.Select(c =>
					{
						c.Enabled = _shiftTradeSettings.MaxSeatsValidationEnabled;
						return c;
					});
			}

			if (_shiftTradeSettings.BusinessRuleConfigs == null)
				_shiftTradeSettings.BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] {};

			var businessRuleConfigList = new List<ShiftTradeBusinessRuleConfig>(_shiftTradeSettings.BusinessRuleConfigs);
			foreach (var defaultConfig in defaultConfigs)
			{
				if (_shiftTradeSettings.BusinessRuleConfigs.All(b => b.BusinessRuleType != defaultConfig.BusinessRuleType))
				{
					businessRuleConfigList.Add(defaultConfig);
				}
			}

			_shiftTradeSettings.BusinessRuleConfigs = businessRuleConfigList.ToArray();
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

		private bool isBusinessRuleConfigurable()
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770);
		}
	}
}