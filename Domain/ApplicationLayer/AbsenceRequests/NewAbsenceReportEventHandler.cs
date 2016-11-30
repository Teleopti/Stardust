﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class NewAbsenceReport : IHandleEvent<NewAbsenceReportCreatedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof (NewAbsenceReport));

		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulingResultStateHolderProvider _schedulingResultStateHolderProvider;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IRequestFactory _factory;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;

		private readonly IList<LoadDataAction> _loadDataActions;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private readonly IPersonRepository _personRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public NewAbsenceReport(ICurrentScenario scenarioRepository,
			ISchedulingResultStateHolderProvider schedulingResultStateHolderProvider, IRequestFactory factory,
			IScheduleDifferenceSaver scheduleDictionarySaver,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,
			IPersonRepository personRepository,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
		{
			_scenarioRepository = scenarioRepository;
			_schedulingResultStateHolderProvider = schedulingResultStateHolderProvider;
			_factory = factory;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_loadSchedulesForRequestWithoutResourceCalculation = loadSchedulesForRequestWithoutResourceCalculation;
			_personRepository = personRepository;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;

			_loadDataActions = new List<LoadDataAction>
			{
				loadDefaultScenario
			};
			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of consumer was created");
			}
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(NewAbsenceReportCreatedEvent message)
		{
			_schedulingResultStateHolder = _schedulingResultStateHolderProvider.GiveMeANew();

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming message for person absence report with Id = {0}. (Message timestamp = {1})",
					message.AbsenceId, message.Timestamp);
			}


			foreach (var action in _loadDataActions)
			{
				if (!action.Invoke(message))
				{
					clearStateHolder();
					return;
				}
			}
			var person = _personRepository.FindPeople(new List<Guid> {message.PersonId}).Single();
			var agentTimeZone = person.PermissionInformation.DefaultTimeZone();

			//create one full day period
			var fullDayTimeSpanEnd = new TimeSpan(23, 59, 0);
			var startDateTime = message.RequestedDate.Date;
			var endDateTime = message.RequestedDate.Date.Add(fullDayTimeSpanEnd);
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDateTime, endDateTime, agentTimeZone);

			if (person.WorkflowControlSet == null)
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat(CultureInfo.CurrentCulture,
						"No workflow control set defined for {0}, {1} (PersonId = {2}). The reported absence with Id = {3} will not be processed.",
						person.EmploymentNumber, person.Name, person.Id, message.AbsenceId);
				}
			}
			else
			{
				var absenceId = message.AbsenceId;
				var reportedAbsence = person.WorkflowControlSet.AllowedAbsencesForReport.FirstOrDefault(a => a.Id == absenceId);
				if (reportedAbsence == null)
				{
					logger.InfoFormat(
						"No valid reportable absence found in message, nothing will be done. PersonId: {0}, Request Date: {1:yyyy-MM-dd}, Absence Id: {2}",
						message.PersonId, message.RequestedDate, message.AbsenceId);
				}
				else
				{
					var undoRedoContainer = new UndoRedoContainer();
                    loadDataForResourceCalculation(period, person);

					_schedulingResultStateHolder.Schedules.TakeSnapshot();
					_schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);

					var businessRules =
						_businessRulesForPersonalAccountUpdate.FromScheduleRange(_schedulingResultStateHolder.Schedules[person]);

					var requestApprovalServiceScheduler = _factory.GetRequestApprovalService(businessRules,
						_scenarioRepository.Current(), _schedulingResultStateHolder);

					var brokenBusinessRules = requestApprovalServiceScheduler.ApproveAbsence(reportedAbsence, period, person);

					if (logger.IsDebugEnabled)
					{
						if (brokenBusinessRules != null)
						{
							foreach (var brokenBusinessRule in brokenBusinessRules)
							{
								logger.DebugFormat("A rule was broken: {0}", brokenBusinessRule.Message);
							}
						}

						logger.Debug("Simulated approving absence successfully");
					}
				
					var approvedPersonAbsence = requestApprovalServiceScheduler.GetApprovedPersonAbsence();
					approvedPersonAbsence?.FullDayAbsence(person,new TrackedCommandInfo
					{
						OperatedPersonId = person.Id.Value,
						TrackId = Guid.NewGuid()
					});

					try
					{
						persistScheduleChanges(person);
					}
					catch (ValidationException validationException)
					{
						logger.Error("A validation error occurred. Review the error log. Processing cannot continue.",
							validationException);
						clearStateHolder();
						return;
					}
				}
			}

			clearStateHolder();
		}

		private void persistScheduleChanges(IPerson person)
		{
			_scheduleDictionarySaver.SaveChanges(_schedulingResultStateHolder.Schedules.DifferenceSinceSnapshot(),
				(IUnvalidatedScheduleRangeUpdate) _schedulingResultStateHolder.Schedules[person]);
		}

		private void clearStateHolder()
		{
			_schedulingResultStateHolder.Dispose();
			_schedulingResultStateHolder = null;
		}

		private void loadDataForResourceCalculation(DateTimePeriod period, IPerson person)
		{
			DateTimePeriod periodForResourceCalc = period.ChangeStartTime(TimeSpan.FromDays(-1));
			_loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
				periodForResourceCalc,
				new List<IPerson> {person}, _schedulingResultStateHolder);
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
					periodForResourceCalc);
			}
		}

		private bool loadDefaultScenario(NewAbsenceReportCreatedEvent message)
		{
			var defaultScenario = _scenarioRepository.Current();
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
					defaultScenario.Id);
			}
			return true;
		}

		private delegate bool LoadDataAction(NewAbsenceReportCreatedEvent message);
	}
}
