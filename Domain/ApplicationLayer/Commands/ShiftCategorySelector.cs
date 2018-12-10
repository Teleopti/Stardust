using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ShiftCategorySelector : IShiftCategorySelector
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private static readonly ShiftCategorySorter shiftCategorySorter = new ShiftCategorySorter();

		public ShiftCategorySelector(IShiftCategoryRepository shiftCategoryRepository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
		}

		public IShiftCategory Get(IPerson person, DateOnly date, DateTimePeriod shiftPeriod)
		{
			var shiftCategories = _shiftCategoryRepository.FindAll().ToArray();
			Array.Sort(shiftCategories, shiftCategorySorter);
			return shiftCategories.FirstOrDefault();
		}
	}
}