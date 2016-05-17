using System;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Absence
{
	[EnabledBy(Toggles.ETL_SpeedUpAbsence_38301)]
	public class AbsenceAnalyticsUpdater:
		IHandleEvent<AbsenceChangedEvent>,
		IHandleEvent<AbsenceDeletedEvent>,
		IRunOnHangfire
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(AbsenceAnalyticsUpdater));
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AbsenceAnalyticsUpdater(IAbsenceRepository absenceRepository, IAnalyticsAbsenceRepository analyticsAbsenceRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_absenceRepository = absenceRepository;
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public void Handle(AbsenceChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AbsenceChangedEvent)} for absence id = {@event.AbsenceId}. (Message timestamp = {@event.Timestamp})");
			var absence = _absenceRepository.Load(@event.AbsenceId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			var analyticsAbsence = _analyticsAbsenceRepository.Absences().FirstOrDefault(a => a.AbsenceCode == @event.AbsenceId);

			if (absence == null || analyticsBusinessUnit == null)
				return;

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

		private AnalyticsAbsence transformToAnalyticsAbsence(IAbsence absence, int analyticsBusinessUnitId)
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
		public void Handle(AbsenceDeletedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AbsenceDeletedEvent)} for absence id = {@event.AbsenceId}. (Message timestamp = {@event.Timestamp})");
			var analyticsAbsence = _analyticsAbsenceRepository.Absences().FirstOrDefault(a => a.AbsenceCode == @event.AbsenceId);

			if (analyticsAbsence == null)
				return;

			// Delete
			_analyticsAbsenceRepository.UpdateAbsence(new AnalyticsAbsence
			{
				AbsenceCode = analyticsAbsence.AbsenceCode,
				AbsenceName = analyticsAbsence.AbsenceName,
				AbsenceShortName = analyticsAbsence.AbsenceShortName,
				DisplayColor = analyticsAbsence.DisplayColor,
				DisplayColorHtml = analyticsAbsence.DisplayColorHtml,
				BusinessUnitId = analyticsAbsence.BusinessUnitId,
				InContractTime = analyticsAbsence.InContractTime,
				InContractTimeName = analyticsAbsence.InContractTimeName,
				InPaidTime = analyticsAbsence.InPaidTime,
				InPaidTimeName = analyticsAbsence.InPaidTimeName,
				InWorkTime = analyticsAbsence.InWorkTime,
				InWorkTimeName = analyticsAbsence.InWorkTimeName,
				DatasourceId = 1,
				DatasourceUpdateDate = analyticsAbsence.DatasourceUpdateDate,
				IsDeleted = true
			});
		}
	}
}