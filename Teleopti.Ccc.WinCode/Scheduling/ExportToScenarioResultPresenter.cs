using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class ExportToScenarioResultPresenter
	{
		private readonly IExportToScenarioResultView _view;
		private readonly IScenario _exportScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IMoveDataBetweenSchedules _moveSchedules;
		private readonly IReassociateDataForSchedules _callback;
		private readonly IEnumerable<IPerson> _fullyLoadedPersonsToMove;
		private readonly IEnumerable<IScheduleDay> _schedulePartsToExport;
		private readonly IScheduleDictionaryPersister _scheduleDictionaryBatchPersister;
		private readonly BackgroundWorker _exportBackrgroundWorker = new BackgroundWorker();
		private bool _exportSuccesful;
		private IEnumerable<IBusinessRuleResponse> _warnings;
		private DataSourceException _exception;

		public IScheduleDictionary ScheduleDictionaryToPersist { get; private set; }

		//ingen modell till denna då formuläret inte har något
		//modifierbart state
		public ExportToScenarioResultPresenter(IUnitOfWorkFactory uowFactory,
												IExportToScenarioResultView view,
												IScheduleStorage scheduleStorage,
												IMoveDataBetweenSchedules moveSchedules,
												IReassociateDataForSchedules callback,
												IEnumerable<IPerson> fullyLoadedPersonsToMove,
												IEnumerable<IScheduleDay> schedulePartsToExport,
												IScenario exportScenario,
												IScheduleDictionaryPersister scheduleDictionaryBatchPersister)
		{
			_uowFactory = uowFactory;
			_moveSchedules = moveSchedules;
			_callback = callback;
			_fullyLoadedPersonsToMove = fullyLoadedPersonsToMove;
			_scheduleStorage = scheduleStorage;
			_view = view;
			_exportScenario = exportScenario;
			_schedulePartsToExport = schedulePartsToExport;
			_scheduleDictionaryBatchPersister = scheduleDictionaryBatchPersister;
		}

		protected void SetScheduleDictionaryToPersist(IScheduleDictionary dictionary)
		{
			ScheduleDictionaryToPersist = dictionary;
		}

		public void Initialize()
		{
			if (_schedulePartsToExport.Count() == 0)
			{
				OnCancel();
				return;
			}
			_exportBackrgroundWorker.DoWork += exportBackrgroundWorkerDoWork;
			_exportBackrgroundWorker.RunWorkerCompleted += exportBackrgroundWorkerRunWorkerCompleted;
			PrepareForExport();
			_exportBackrgroundWorker.RunWorkerAsync();


		}

		void exportBackrgroundWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			_exportSuccesful = CommitExport();
		}

		void exportBackrgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_exportBackrgroundWorker.DoWork -= exportBackrgroundWorkerDoWork;
			_exportBackrgroundWorker.RunWorkerCompleted -= exportBackrgroundWorkerRunWorkerCompleted;
			ExportFinished(_exportSuccesful);
		}

		public void OnCancel()
		{
			_view.CloseForm();
		}

		public void OnConfirm()
		{
			try
			{
				_scheduleDictionaryBatchPersister.Persist(ScheduleDictionaryToPersist);
			}
			catch (DataSourceException exception)
			{
				_view.ShowDataSourceException(exception);
			}
			_view.CloseForm();
		}

		private int noOfAgentsInvolved()
		{
			ICollection<IPerson> agents = new HashSet<IPerson>();
			foreach (var part in _schedulePartsToExport)
			{
				agents.Add(part.Person);
			}
			return agents.Count;
		}

		private static IEnumerable<ExportToScenarioWarningData> buildWarningStrings(IEnumerable<IBusinessRuleResponse> ruleResponses)
		{
			var warningStrings = new HashSet<ExportToScenarioWarningData>();
			foreach (var rule in ruleResponses)
			{
				warningStrings.Add(new ExportToScenarioWarningData(rule.Person.Name.ToString(), rule.Message));
			}
			return warningStrings;
		}

		private IEnumerable<IBusinessRuleResponse> doExport()
		{
			using (_uowFactory.CreateAndOpenUnitOfWork())
			{
				var personProvider = new PersonProvider(_fullyLoadedPersonsToMove);
				var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, true);
				_callback.ReassociateDataForAllPeople();
				ScheduleDictionaryToPersist =
					_scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(schedulePartPeriod(), _fullyLoadedPersonsToMove), _exportScenario, personProvider, scheduleDictionaryLoadOptions, _fullyLoadedPersonsToMove);
				return _moveSchedules.CopySchedulePartsToAnotherDictionary(ScheduleDictionaryToPersist, _schedulePartsToExport);
			}
		}

		private DateTimePeriod schedulePartPeriod()
		{
			DateTime earliest = DateTime.MaxValue;
			DateTime latest = DateTime.MinValue;
			foreach (var part in _schedulePartsToExport)
			{
				if (part.Period.StartDateTime < earliest)
					earliest = part.Period.StartDateTime;
				if (part.Period.EndDateTime > latest)
					latest = part.Period.EndDateTime;
			}
			return new DateTimePeriod(earliest, latest);
		}

		private void verifyNecessaryPersons()
		{
			foreach (var part in _schedulePartsToExport)
			{
				if (!_fullyLoadedPersonsToMove.Contains(part.Person))
					throw new ArgumentException("Person " + part.Person + " not provided.");
			}
		}

		public void PrepareForExport()
		{
			var culture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
			_view.SetScenarioText(string.Format(culture,
												Resources.ExportFromAndToScenario,
												_schedulePartsToExport.First().Scenario.Description.Name,
												_exportScenario.Description.Name));
			_view.SetAgentText(string.Format(culture, Resources.ExportNoOfAgentInfo, noOfAgentsInvolved()));
			_view.DisableInteractions();
		}

		public bool CommitExport()
		{
			verifyNecessaryPersons();
			try
			{
				_warnings = doExport();

			}
			catch (DataSourceException exception)
			{
				_exception = exception;
				return false;
			}
			return true;
		}

		public void ExportFinished(bool exportSuccessful)
		{
			if (exportSuccessful)
			{
				_view.EnableInteractions();

				if (_warnings != null && _warnings.Count() > 0)
				{
					var warningStrings = buildWarningStrings(_warnings);
					_view.SetWarningText(warningStrings);
				}
				else
				{
					_view.DisableBodyText();
				}
			}
			else
			{
				_view.ShowDataSourceException(_exception);
				_view.CloseForm();
			}
		}
	}
}