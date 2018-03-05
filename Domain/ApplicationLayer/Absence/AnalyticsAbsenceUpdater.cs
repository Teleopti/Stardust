using System;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Absence
{
	public class AnalyticsAbsenceUpdater :
		IHandleEvent<AbsenceChangedEvent>,
		IHandleEvent<AbsenceDeletedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsAbsenceUpdater));
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AnalyticsAbsenceUpdater(IAbsenceRepository absenceRepository, IAnalyticsAbsenceRepository analyticsAbsenceRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_absenceRepository = absenceRepository;
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AbsenceChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AbsenceChangedEvent)} for absence id = {@event.AbsenceId}. (Message timestamp = {@event.Timestamp})");
			var absence = _absenceRepository.Load(@event.AbsenceId);
			if (absence == null)
			{
				logger.Warn($"Absence '{@event.AbsenceId}' was not found in applicaton database.");
				return;
			}

			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			var analyticsAbsence = _analyticsAbsenceRepository.Absence(@event.AbsenceId);

			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			// Add
			if (analyticsAbsence == null)
			{
				_analyticsAbsenceRepository.AddAbsence(transformToAnalyticsAbsence(absence, analyticsBusinessUnit.BusinessUnitId));
			}
			// Update
			else
			{
				_analyticsAbsenceRepository.UpdateAbsence(transformToAnalyticsAbsence(absence, analyticsBusinessUnit.BusinessUnitId));
			}
		}

		private static AnalyticsAbsence transformToAnalyticsAbsence(IAbsence absence, int analyticsBusinessUnitId)
		{
			return new AnalyticsAbsence
			{
				AbsenceCode = absence.Id.GetValueOrDefault(),
				AbsenceName = absence.Description.Name,
				AbsenceShortName = absence.Description.ShortName,
				DisplayColor = absence.DisplayColor.ToArgb(),
				DisplayColorHtml = ColorTranslator.ToHtml(absence.DisplayColor),
				BusinessUnitId = analyticsBusinessUnitId,
				InContractTime = absence.InContractTime,
				InContractTimeName = absence.InContractTime ? AnalyticsAbsence.InContractTimeString : AnalyticsAbsence.NotInContractTimeString,
				InPaidTime = absence.InPaidTime,
				InPaidTimeName = absence.InPaidTime ? AnalyticsAbsence.InPaidTimeTimeString : AnalyticsAbsence.NotInPaidTimeTimeString,
				InWorkTime = absence.InWorkTime,
				InWorkTimeName = absence.InWorkTime ? AnalyticsAbsence.InWorkTimeTimeString : AnalyticsAbsence.NotInWorkTimeTimeString,
				DatasourceId = 1,
				DatasourceUpdateDate = absence.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
				IsDeleted = false
			};
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AbsenceDeletedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AbsenceDeletedEvent)} for absence id = {@event.AbsenceId}. (Message timestamp = {@event.Timestamp})");
			var analyticsAbsence = _analyticsAbsenceRepository.Absence(@event.AbsenceId);

			if (analyticsAbsence == null)
				return;

			analyticsAbsence.IsDeleted = true;
			_analyticsAbsenceRepository.UpdateAbsence(analyticsAbsence);
		}
	}
}