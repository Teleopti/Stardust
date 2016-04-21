using System;
using System.Collections.Generic;
using NPOI.HPSF;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public class AbsencePersister : IAbsencePersister
	{
		private readonly IAbsenceCommandConverter _absenceCommandConverter;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;
		private readonly IPermissionChecker _permissionChecker;

		public AbsencePersister(IAbsenceCommandConverter absenceCommandConverter, IPersonAbsenceCreator personAbsenceCreator,
			IPermissionChecker permissionChecker)
		{
			_absenceCommandConverter = absenceCommandConverter;
			_personAbsenceCreator = personAbsenceCreator;
			_permissionChecker = permissionChecker;
		}

		public FailActionResult PersistFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForFullDayAbsence(command);

			var checkResult = _permissionChecker.CheckAddFullDayAbsenceForPerson(absenceCreatorInfo.Person, new DateOnly(command.StartDate));
			if (checkResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), new List<string>{checkResult});
			
			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, true);
			if (createResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), createResult);

			return null;
		}

		public FailActionResult PersistIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForIntradayAbsence(command);

			var checkResult = _permissionChecker.CheckAddIntradayAbsenceForPerson(absenceCreatorInfo.Person, new DateOnly(command.StartTime));
			if (checkResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), new List<string> { checkResult });

			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, false);
			if (createResult != null) return getFailActionResult(absenceCreatorInfo.Person.Id.GetValueOrDefault(), createResult);

			return null;
		}

		private FailActionResult getFailActionResult(Guid personId, IList<string> message)
		{
			return new FailActionResult
			{
				PersonId = personId,
				Messages = message
			};
		}
	}
}