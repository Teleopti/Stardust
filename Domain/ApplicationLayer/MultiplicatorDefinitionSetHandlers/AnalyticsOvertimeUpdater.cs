﻿using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers
{
	public class AnalyticsOvertimeUpdater : 
		IRunOnHangfire,
		IHandleEvent<MultiplicatorDefinitionSetCreated>,
		IHandleEvent<MultiplicatorDefinitionSetChanged>,
		IHandleEvent<MultiplicatorDefinitionSetDeleted>
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsOvertimeUpdater));

		private readonly IAnalyticsOvertimeRepository _analyticsOvertimeRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

		public AnalyticsOvertimeUpdater(IAnalyticsOvertimeRepository analyticsOvertimeRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
		{
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			logger.Debug($"New instance of {nameof(AnalyticsOvertimeUpdater)} was created");
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(MultiplicatorDefinitionSetCreated @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetCreated)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(MultiplicatorDefinitionSetChanged @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetChanged)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(MultiplicatorDefinitionSetDeleted @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetDeleted)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		private void handle(MultiplicatorDefinitionSetChangedBase @event)
		{
			if (@event.MultiplicatorType != MultiplicatorType.Overtime)
			{
				logger.Debug($"Not {nameof(MultiplicatorType.Overtime)}, ending processing.");
				return;
			}
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetRepository.Get(@event.MultiplicatorDefinitionSetId);
			if (multiplicatorDefinitionSet == null)
			{
				logger.Warn($"{nameof(IMultiplicatorDefinitionSet)} '{@event.MultiplicatorDefinitionSetId}' did not exists in application.");
				return;
			}
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			_analyticsOvertimeRepository.AddOrUpdate(new AnalyticsOvertime
			{
				BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
				OvertimeCode = @event.MultiplicatorDefinitionSetId,
				OvertimeName = @multiplicatorDefinitionSet.Name,
				DatasourceUpdateDate = multiplicatorDefinitionSet.UpdatedOn ?? DateTime.UtcNow,
				IsDeleted = multiplicatorDefinitionSet.IsDeleted
			});
		}
	}
}