using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public class AnalyticsShiftCategoryUpdater :
		IHandleEvent<ShiftCategoryChangedEvent>,
		IHandleEvent<ShiftCategoryDeletedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsShiftCategoryUpdater));
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IAnalyticsShiftCategoryRepository _analyticsShiftCategoryRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AnalyticsShiftCategoryUpdater(IShiftCategoryRepository shiftCategoryRepository, IAnalyticsShiftCategoryRepository analyticsShiftCategoryRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_analyticsShiftCategoryRepository = analyticsShiftCategoryRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ShiftCategoryChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AnalyticsShiftCategoryUpdater)} for shiftCategory id = {@event.ShiftCategoryId}. (Message timestamp = {@event.Timestamp})");
			var shiftCategory = _shiftCategoryRepository.Load(@event.ShiftCategoryId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			var analyticsShiftCategory = _analyticsShiftCategoryRepository.ShiftCategory(@event.ShiftCategoryId);

			if (shiftCategory == null)
			{
				logger.Warn($"ShiftCategory '{@event.ShiftCategoryId}' was not found in application.");
				return;
			}

			// Add
			if (analyticsShiftCategory == null)
			{
				_analyticsShiftCategoryRepository.AddShiftCategory(transformToAnalyticsShiftCategory(shiftCategory, analyticsBusinessUnit.BusinessUnitId));
			}
			// Update
			else
			{
				_analyticsShiftCategoryRepository.UpdateShiftCategory(transformToAnalyticsShiftCategory(shiftCategory, analyticsBusinessUnit.BusinessUnitId));
			}
		}

		private static AnalyticsShiftCategory transformToAnalyticsShiftCategory(IShiftCategory shiftCategory, int businessUnitId)
		{
			return new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategory.Id.GetValueOrDefault(),
				ShiftCategoryName = shiftCategory.Description.Name,
				ShiftCategoryShortname = shiftCategory.Description.ShortName,
				DisplayColor = shiftCategory.DisplayColor.ToArgb(),
				BusinessUnitId = businessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = shiftCategory.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
				IsDeleted = false
			};
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ShiftCategoryDeletedEvent @event)
		{
			logger.Debug($"Consuming {nameof(ShiftCategoryDeletedEvent)} for shift category id = {@event.ShiftCategoryId}. (Message timestamp = {@event.Timestamp})");
			var analyticsShiftCategory = _analyticsShiftCategoryRepository.ShiftCategory(@event.ShiftCategoryId);

			if (analyticsShiftCategory == null)
				return;

			// Delete
			_analyticsShiftCategoryRepository.UpdateShiftCategory(new AnalyticsShiftCategory
			{
				ShiftCategoryCode = analyticsShiftCategory.ShiftCategoryCode,
				ShiftCategoryName = analyticsShiftCategory.ShiftCategoryName,
				ShiftCategoryShortname = analyticsShiftCategory.ShiftCategoryShortname,
				DisplayColor = analyticsShiftCategory.DisplayColor,
				BusinessUnitId = analyticsShiftCategory.BusinessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = analyticsShiftCategory.DatasourceUpdateDate,
				IsDeleted = true
			});
		}
	}
}