using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class ShiftCategoryProvider : IShiftCategoryProvider
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;

		public ShiftCategoryProvider(IShiftCategoryRepository shiftCategoryRepository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
		}

		public IList<ShiftCategoryViewModel> GetAll()
		{
			var shiftCategories = _shiftCategoryRepository.LoadAll()
				.OrderByDescending(x => x.Rank.HasValue ? x.Rank : int.MaxValue).ThenByDescending(x => x.Description.Name);

			return shiftCategories.Select(a => new ShiftCategoryViewModel
			{
				Id = a.Id,
				ShortName = a.Description.ShortName,
				Name = a.Description.Name,
				DisplayColor = a.DisplayColor.ToHtml()
			}).ToList();
		}
	}


}