using System;
using System.Drawing;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.DayOff
{
	public class AnalyticsDayOffUpdater :
		IHandleEvent<DayOffTemplateChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsDayOffUpdater));

		public AnalyticsDayOffUpdater(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsDayOffRepository analyticsDayOffRepository, IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsDayOffRepository = analyticsDayOffRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			logger.Info($"New instance of {nameof(AnalyticsDayOffUpdater)} was created");
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(DayOffTemplateChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(DayOffTemplateChangedEvent)} for day off template id = {@event.DayOffTemplateId}.");

			var businessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			var dayOffTemplate = _dayOffTemplateRepository.Get(@event.DayOffTemplateId);
			if (dayOffTemplate == null)
			{
				logger.Warn($"DayOff '{@event.DayOffTemplateId}' was not found in application database.");
				return;
			}
			var dayOff = new AnalyticsDayOff
			{
				DayOffCode = @event.DayOffTemplateId,
				BusinessUnitId = businessUnit.BusinessUnitId,
				DayOffName = dayOffTemplate.Description.Name,
				DayOffShortname = dayOffTemplate.Description.ShortName,
				DisplayColor = dayOffTemplate.DisplayColor.ToArgb(),
				DisplayColorHtml = ColorTranslator.ToHtml(dayOffTemplate.DisplayColor),
				DatasourceUpdateDate = DateHelper.GetSmallDateTime(dayOffTemplate.UpdatedOn ?? DateTime.UtcNow),
				DatasourceId = 1
			};

			_analyticsDayOffRepository.AddOrUpdate(dayOff);
		}
	}
}
