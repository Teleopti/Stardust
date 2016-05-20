using System;
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
	[EnabledBy(Toggles.ETL_SpeedUpIntradayShiftCategory_38718)]
	public class AnalyticsShiftCategoryUpdater :
		IHandleEvent<ShiftCategoryChangedEvent>,
		IHandleEvent<ShiftCategoryDeletedEvent>,
		IRunOnHangfire
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(AnalyticsShiftCategoryUpdater));
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
		public virtual void Handle(ShiftCategoryChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(AnalyticsShiftCategoryUpdater)} for shiftCategory id = {@event.ShiftCategoryId}. (Message timestamp = {@event.Timestamp})");
			var shiftCategory = _shiftCategoryRepository.Load(@event.ShiftCategoryId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			var analyticsShiftCategory = _analyticsShiftCategoryRepository.ShiftCategories().FirstOrDefault(a => a.ShiftCategoryCode == @event.ShiftCategoryId);

			if (shiftCategory == null || analyticsBusinessUnit == null)
				return;

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

		private AnalyticsShiftCategory transformToAnalyticsShiftCategory(IShiftCategory shiftCategory, int businessUnitId)
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

		public virtual void Handle(ShiftCategoryDeletedEvent @event)
		{
			//throw new NotImplementedException();
		}
	}
}