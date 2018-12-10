using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ShiftCategorySelectorWithPrediction : IShiftCategorySelector
	{
		private readonly ShiftCategorySelector _shiftCategorySelector;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IRepository<IShiftCategorySelection> _selectionRepository;
		private readonly IShiftCategoryPredictionModelLoader _predictionModelLoader;

		public ShiftCategorySelectorWithPrediction(ShiftCategorySelector shiftCategorySelector,
			IShiftCategoryRepository shiftCategoryRepository, IRepository<IShiftCategorySelection> selectionRepository,
			IShiftCategoryPredictionModelLoader predictionModelLoader)
		{
			_shiftCategorySelector = shiftCategorySelector;
			_shiftCategoryRepository = shiftCategoryRepository;
			_selectionRepository = selectionRepository;
			_predictionModelLoader = predictionModelLoader;
		}

		public IShiftCategory Get(IPerson person, DateOnly date, DateTimePeriod shiftPeriod)
		{
			var model = _selectionRepository.LoadAll().FirstOrDefault();
			if (!string.IsNullOrEmpty(model?.Model))
			{
				var localPeriod = shiftPeriod.TimePeriod(person.PermissionInformation.DefaultTimeZone());

				var m = _predictionModelLoader.Load(model.Model);
				try
				{
					var category = m.Predict(new ShiftCategoryExample
					{
						DayOfWeek = date.DayOfWeek,
						StartTime = localPeriod.StartTime.TotalHours,
						EndTime = localPeriod.EndTime.TotalHours
					});
					if (Guid.TryParse(category, out var id))
						return _shiftCategoryRepository.Get(id);
				}
				catch(InvalidOperationException)
				{ }
			}

			return _shiftCategorySelector.Get(person, date, shiftPeriod);
		}
	}
}