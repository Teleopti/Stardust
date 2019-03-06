using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public class AbsencePersister : IAbsencePersister
	{
		private readonly IAbsenceCommandConverter _absenceCommandConverter;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly IPermissionChecker _permissionChecker;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentDataSource _currentDataSource;

		public AbsencePersister(IAbsenceCommandConverter absenceCommandConverter,
			IPersonAbsenceCreator personAbsenceCreator,
			IPermissionChecker permissionChecker,
			IEventPublisher eventPublisher,
			ICurrentUnitOfWork currentUnitOfWork,
			ICurrentScenario currentScenario,
			ICurrentDataSource currentDataSource)
		{
			_absenceCommandConverter = absenceCommandConverter;
			_personAbsenceCreator = personAbsenceCreator;
			_permissionChecker = permissionChecker;
			_eventPublisher = eventPublisher;
			_currentUnitOfWork = currentUnitOfWork;
			_currentScenario = currentScenario;
			_currentDataSource = currentDataSource;
		}

		public ActionResult PersistFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForFullDayAbsence(command);

			var checkResult = _permissionChecker.CheckAddFullDayAbsenceForPerson(absenceCreatorInfo.Person, new DateOnly(command.StartDate));
			if (checkResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), new List<string> { checkResult });

			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, true);
			if (createResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), createResult);

			return null;
		}

		public ActionResult PersistIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForIntradayAbsence(command);

			var checkResult = _permissionChecker.CheckAddIntradayAbsenceForPerson(absenceCreatorInfo.Person, new DateOnly(command.StartTime));
			if (checkResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), new List<string> { checkResult });

			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, false, true);
			if (createResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), createResult);

			var currentScenario = _currentScenario.Current();
			var personAbsenceAddedEvent = new PersonAbsenceAddedEvent
			{
				AbsenceId = command.AbsenceId,
				PersonId = command.PersonId,
				StartDateTime = absenceCreatorInfo.EventTimePeriod.StartDateTime,
				EndDateTime = absenceCreatorInfo.EventTimePeriod.EndDateTime,
				ScenarioId = currentScenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = currentScenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
				LogOnDatasource = _currentDataSource.CurrentName()
			};
			if (command.TrackedCommandInfo != null)
			{
				personAbsenceAddedEvent.InitiatorId = command.TrackedCommandInfo.OperatedPersonId;
				personAbsenceAddedEvent.CommandId = command.TrackedCommandInfo.TrackId;
			}

			_currentUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				_eventPublisher.Publish(personAbsenceAddedEvent);
			});

			return null;
		}

		private ActionResult getFailActionResult(Guid personId, IList<string> message)
		{
			return new ActionResult(personId)
			{
				ErrorMessages = message
			};
		}
	}
}