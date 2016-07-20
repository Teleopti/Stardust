using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class ShiftCategoryProviderTest
	{
		[Test]
		public void ShouldReturnShiftCategories()
		{
			var shiftCategoryRepo = new FakeShiftCategoryRepository();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sc");
			shiftCategory.WithId();

			shiftCategoryRepo.Add(shiftCategory);

			var target = new ShiftCategoryProvider(shiftCategoryRepo);

			var result = target.GetAll();
			result.Count.Should().Be.EqualTo(1);

			result.First().Id.Should().Be.EqualTo(shiftCategory.Id);
			result.First().DisplayColor.Should().Be.EqualTo(shiftCategory.DisplayColor.ToHtml());
			result.First().Name.Should().Be.EqualTo(shiftCategory.Description.Name);
			result.First().ShortName.Should().Be.EqualTo(shiftCategory.Description.ShortName);

		}

		[Test]
		public void ShouldReturnShiftCategoriesInCorrectOrder()
		{
			var shiftCategoryRepo = new FakeShiftCategoryRepository();

			var shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("Day");
			shiftCategory1.WithId();
			shiftCategory1.Rank = 3;
			var shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("Early");
			shiftCategory2.WithId();
			shiftCategory2.Rank = 2;
			var shiftCategory3 = ShiftCategoryFactory.CreateShiftCategory("Night");
			shiftCategory3.WithId();
			shiftCategory3.Rank = 1;

			shiftCategoryRepo.Add(shiftCategory1);
			shiftCategoryRepo.Add(shiftCategory2);
			shiftCategoryRepo.Add(shiftCategory3);

			var target = new ShiftCategoryProvider(shiftCategoryRepo);
			var result = target.GetAll();

			result.Count.Should().Be.EqualTo(3);
			result.First().Name.Should().Be.EqualTo("Day");
			result.Last().Name.Should().Be.EqualTo("Night");
		}
	}
}
