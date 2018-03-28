using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers
{
	public class ShiftCategorySelectionModelUpdater : IHandleEvent<TenantDayTickEvent>, IHandleEvent<ShiftCategoryDeletedEvent>, IRunOnHangfire
	{
		private readonly IRepository<IShiftCategorySelection> _shiftCategorySelectionRepository;
		private readonly IShiftCategoryUsageFinder _shiftCategoryUsageFinder;
		private readonly IPredictShiftCategory _predictShiftCategory;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly IUpdatedBySystemUser _updatedBySystemUser;
		private readonly IBusinessUnitScope _businessUnitScope;

		public ShiftCategorySelectionModelUpdater(IRepository<IShiftCategorySelection> shiftCategorySelectionRepository,
			IShiftCategoryUsageFinder shiftCategoryUsageFinder, IPredictShiftCategory predictShiftCategory,
			IBusinessUnitRepository businessUnitRepository, IUpdatedBySystemUser updatedBySystemUser,
			IBusinessUnitScope businessUnitScope)
		{
			_shiftCategorySelectionRepository = shiftCategorySelectionRepository;
			_shiftCategoryUsageFinder = shiftCategoryUsageFinder;
			_predictShiftCategory = predictShiftCategory;
			_businessUnitRepository = businessUnitRepository;
			_updatedBySystemUser = updatedBySystemUser;
			_businessUnitScope = businessUnitScope;
		}

		[UnitOfWork]
		public virtual void Handle(TenantDayTickEvent @event)
		{
			var businessUnits = _businessUnitRepository.LoadAll();
			using (_updatedBySystemUser.Context())
			{
				businessUnits.ForEach(businessUnit =>
				{
					_businessUnitScope.OnThisThreadUse(businessUnit);
					updateShiftCategoryPredictionModel();
				});
			}
		}

		private void updateShiftCategoryPredictionModel()
		{
			string model = string.Empty;
			var history = _shiftCategoryUsageFinder.Find();
			if (history.Any())
			{
				var result = _predictShiftCategory.Train(history);
				using (var ms = new MemoryStream())
				{
					result.Store(ms);
					ms.Position = 0;
					using (var reader = new StreamReader(ms))
					{
						model = reader.ReadToEnd();
					}
				}
			}

			var currentModel = _shiftCategorySelectionRepository.LoadAll().FirstOrDefault();
			if (currentModel == null)
			{
				_shiftCategorySelectionRepository.Add(new ShiftCategorySelection {Model = model});
			}
			else
			{
				currentModel.Model = model;
			}
		}

		[UnitOfWork]
		public virtual void Handle(ShiftCategoryDeletedEvent @event)
		{
			updateShiftCategoryPredictionModel();
		}
	}
}