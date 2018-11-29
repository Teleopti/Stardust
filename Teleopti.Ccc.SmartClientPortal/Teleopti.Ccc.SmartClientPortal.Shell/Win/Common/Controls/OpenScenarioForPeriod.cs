using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	public partial class OpenScenarioForPeriod : BaseDialogForm
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(OpenScenarioForPeriod));
		private OpenScenarioForPeriodSetting _setting;
		private bool _shrinkage;
		private bool _calculation;
		private bool _validation;
		private bool _requests;
		private IList<IScenario> _scenarios;
		private bool _forceClose;
		private readonly IOpenPeriodMode _openMode;
		private bool _teamLeaderMode;
		private readonly IList<IEntity> _selectedEntityList;

		public OpenScenarioForPeriod(IOpenPeriodMode openMode)
		{
			_openMode = openMode;

			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
				dateSelectionControl1.DateRangeChanged += dateSelectionControl1DateRangeChanged;
				validateSelectedDatesControl(dateSelectionControl1.GetCurrentlySelectedDates());
			}

			groupBox1.Visible = !_openMode.ForecasterStyle;
			
		}

		public OpenScenarioForPeriod(IOpenPeriodMode openMode, IList<IEntity> selectedEntityList)
			:this(openMode)
		{
			_selectedEntityList = new List<IEntity>(selectedEntityList);
			checkBoxRequests.Visible = PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler);
		}

		private bool validateSelectedDatesControl(ICollection<DateOnlyPeriod> periods)
		{
			buttonOK.Enabled = false;
			if (periods.Count == 0)
			{
				dateSelectionControl1.SetErrorOnDateSelectionFromTo(Resources.StartDateMustBeSmallerThanEndDate);
				return false;
			}

			var period = periods.ElementAtOrDefault(0);
			//var selectedDates = dateSelectionControl1.GetCurrentlySelectedDates();
			var openPeriodSpecification = _openMode.Specification;
			if (period.StartDate > period.EndDate)
			{
				dateSelectionControl1.SetErrorOnDateSelectionFromTo(Resources.StartDateMustBeSmallerThanEndDate);
				return false;
			}
			if (!openPeriodSpecification.IsSatisfiedBy(period))
			{
				var errorMessage = string.Format(CultureInfo.CurrentUICulture, Resources.SelectedPeriodShouldNotBeLongerThanParameterDot, _openMode.AliasOfMaxNumberOfDays);
				dateSelectionControl1.SetErrorOnDateSelectionFromTo(errorMessage);
				return false;
			}
			dateSelectionControl1.SetErrorOnDateSelectionFromTo(string.Empty);
			buttonOK.Enabled = true;
			return true;
		}

		private bool validateScenarioControl(IList<IScenario> scenariosToLoad)
		{
			if (noScenarioAvailable(scenariosToLoad))
			{
				if (hasFunctionPermissionForRestrictedScenarios())
				{
					errorProvider1.SetError(comboBoxScenario, Resources.HavePermissionToRestrictedScenariosButNotForSelectedTeams);
					comboBoxScenario.DataSource = null;
					buttonOK.Enabled = false;
					return false;
				}
			}
			errorProvider1.SetError(comboBoxScenario, String.Empty);
			return true;
		}

		public override string HelpId
		{
			get
			{
				return _openMode.SettingName;
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (_forceClose)
			{
				return;
			}
			if (DialogResult != System.Windows.Forms.DialogResult.Cancel)
			{
				var selectedDates = dateSelectionControl1.GetCurrentlySelectedDates();
				if (selectedDates.Count == 0)
				{
					dateSelectionControl1.SetErrorOnDateSelectionFromTo(Resources.StartDateMustBeSmallerThanEndDate);
					e.Cancel = true;
				}
				else
				{
					_setting.StartDate = selectedDates[0].StartDate.Date;
					_setting.EndDate = selectedDates[0].EndDate.Date;
					_setting.ScenarioId = Scenario.Id.GetValueOrDefault();
					_setting.NoShrinkage = !_shrinkage;
					_setting.NoCalculation = !_calculation;
					_setting.NoValidation = !_validation;
					_setting.NoRequests = !_requests;
					_setting.TeamLeaderMode = _teamLeaderMode;
					try
					{
						saveSetting();
					}
					catch (DataSourceException dataSourceException)
					{
						_logger.Error(Resources.ErrorOccuredWhenSavingSettings, dataSourceException);
						datasourceExceptionOccurred(dataSourceException);
					}
				}
			}
		}
		
		private void saveSetting()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonalSettingDataRepository(uow).PersistSettingValue(_setting);
				uow.PersistAll();
			}
		}

		public bool Shrinkage
		{
			get { return _shrinkage; }
		}
		public bool Validation
		{
			get { return _validation; }
		}
		public bool Calculation
		{
			get { return _calculation; }
		}

		public bool Requests
		{
			get { return _requests; }
		}

		public IScenario Scenario
		{
			get { return (IScenario)comboBoxScenario.SelectedItem; }
		}

		public bool TeamLeaderMode
		{
			get { return _teamLeaderMode; }
		}

		public DateOnlyPeriod SelectedPeriod
		{
			get
			{
				//Workaround for fixing bug: 3427, 
				//If you press Enter when you are inside one of the date selectors.
				//This will select next control before
				//the list of dates are fetched... I am so glad that it works!!! //Peter
				SelectNextControl(buttonOK, true, true, true, true);

				IList<DateOnlyPeriod> dates = dateSelectionControl1.GetCurrentlySelectedDates();
				if (dates.Count > 0)
					return dates[0];
				return new DateOnlyPeriod();
			}
		}

		private void datasourceExceptionOccurred(DataSourceException dataSourceException)
		{
			using (
				var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenTeleoptiCCC,
														  Resources.ServerUnavailable))
			{
				view.ShowDialog();
			}

			_forceClose = true;
			Close();
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			loadScenarios();
			if (noScenarioAvailable(_scenarios))
			{
				Close();
				return;
			}
			IList<IScenario> permittedScenarios = filterPermittedScenarios(SelectedPeriod.StartDate);
			if (noScenarioAvailable(permittedScenarios))
			{
				if (!hasFunctionPermissionForRestrictedScenarios())
				{
					ShowErrorMessage(Resources.NoPermissionToViewRestrictedScenarios, Resources.NoScenario);
					Close();
				}
			}

			checkBoxAdvShrinkage.Checked = !_setting.NoShrinkage;
			checkBoxAdvCalculation.Checked = !_setting.NoCalculation;
			checkBoxAdvValidation.Checked = !_setting.NoValidation;
			checkBoxRequests.Checked = !_setting.NoRequests;

			_teamLeaderMode = _setting.TeamLeaderMode;
			checkBoxAdvLeaderMode.Checked = !_teamLeaderMode;
			checkBoxAdvCalculation.Enabled = checkBoxAdvShrinkage.Enabled = !_teamLeaderMode;
			DateTime startDate = _setting.StartDate;
			DateTime endDate = _setting.EndDate;
			dateSelectionControl1.SetInitialDates(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)));
			buttonOK.Select();

		    if (RightToLeftLayout) Left = Screen.PrimaryScreen.WorkingArea.Right - 200 - Width;
		}

		private void bindScenarioCombo(IEnumerable<IScenario> scenariosToLoad)
		{
			comboBoxScenario.DataSource = null;
			comboBoxScenario.DisplayMember = "Description";
			comboBoxScenario.DataSource = scenariosToLoad;

			if (_setting.ScenarioId.HasValue)
			{
				IScenario scenario = scenariosToLoad.FirstOrDefault(s => s.Id.Value == _setting.ScenarioId.Value);
				if (scenario != null) comboBoxScenario.SelectedItem = scenario;
			}
		}

		private bool hasScenarioDataChanged(IList<IScenario> scenariosToLoad)
		{
			if (comboBoxScenario.DataSource == null)
				return true;

			return (comboBoxScenario.Items.Count != scenariosToLoad.Count);
		}

		private IList<IScenario> filterPermittedScenarios(DateOnly dateOnly)
		{
			IList<IScenario> permittedScenarios = new List<IScenario>(_scenarios);
			if (_openMode.ConsiderRestrictedScenarios)
			{
				removeNotPermittedRestrictedScenarios(permittedScenarios, dateOnly);
			}
			return permittedScenarios;
		}

		private static bool noScenarioAvailable(IList<IScenario> scenarios)
		{
			return scenarios.Count == 0;
		}

		private void loadScenarios()
		{
			try
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					_setting = new PersonalSettingDataRepository(uow).FindValueByKey(_openMode.SettingName, new OpenScenarioForPeriodSetting());
					_scenarios = new ScenarioRepository(uow).FindAllSorted();
				}
			}
			catch (DataSourceException dataSourceException)
			{
				datasourceExceptionOccurred(dataSourceException);
				_scenarios = new List<IScenario>();
			}

			if (noScenarioAvailable(_scenarios))
			{
				ShowErrorMessage(Resources.ScenarioMustBeCreated, Resources.NoScenario);
				_scenarios = new List<IScenario>();
			}
			if (!validateSelectedDatesControl(dateSelectionControl1.GetCurrentlySelectedDates()))
				return;
			DateOnly dateOnly = dateSelectionControl1.GetCurrentlySelectedDates()[0].StartDate;
			checkScenarios(dateOnly);
		}

		private void removeNotPermittedRestrictedScenarios(IList<IScenario> scenarios, DateOnly dateOnly)
		{
			bool permissionToRestrictedScearios = hasDataPermissionForRestrictedScearios(dateOnly);

			for (var i = scenarios.Count - 1; i > -1; i--)
			{
				if (scenarios[i].Restricted && !permissionToRestrictedScearios)
					scenarios.RemoveAt(i);
			}
		}

		private bool hasDataPermissionForRestrictedScearios(DateOnly dateOnly)
		{
			if (_selectedEntityList == null) return true;

			var authorization = PrincipalAuthorization.Current();

			if (_selectedEntityList.OfType<ITeam>().Any()) 
				return _selectedEntityList.OfType<ITeam>().All(team => authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario, dateOnly, team));

			if (_selectedEntityList.OfType<IPerson>().Any()) 
				return _selectedEntityList.OfType<IPerson>().All(person => authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario, dateOnly, person));

			return true;
		}

		private static bool hasFunctionPermissionForRestrictedScenarios()
		{
			var authorization = PrincipalAuthorization.Current();
			return authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario);
		}

		private void checkBoxAdvShrinkageCheckStateChanged(object sender, EventArgs e)
		{
			_shrinkage = checkBoxAdvShrinkage.Checked;
		}

		private void checkBoxAdvCalculationCheckStateChanged(object sender, EventArgs e)
		{
			_calculation = checkBoxAdvCalculation.Checked;
		}

		private void checkBoxAdvValidationCheckStateChanged(object sender, EventArgs e)
		{
			_validation = checkBoxAdvValidation.Checked;
		}

		private void checkBoxAdvLeaderModeCheckStateChanged(object sender, EventArgs e)
		{
			_teamLeaderMode = !checkBoxAdvLeaderMode.Checked;
			checkBoxAdvCalculation.Enabled = checkBoxAdvShrinkage.Enabled = !_teamLeaderMode;
		}

		void dateSelectionControl1DateRangeChanged(object sender, DateSelection.DateRangeChangedEventArgs e)
		{
			if (!validateSelectedDatesControl(e.SelectedDates))
				return;
			DateOnly dateOnly = e.SelectedDates[0].StartDate;
			checkScenarios(dateOnly);
		}

		void checkScenarios(DateOnly dateOnly)
		{
			IList<IScenario> permittedScenarios = filterPermittedScenarios(dateOnly);
			if (!validateScenarioControl(permittedScenarios))
				return;
			if (hasScenarioDataChanged(permittedScenarios))
				bindScenarioCombo(permittedScenarios);
		}

		private void checkBoxRequestsCheckStateChanged(object sender, EventArgs e)
		{
			_requests = checkBoxRequests.Checked;
		}
	}
}
