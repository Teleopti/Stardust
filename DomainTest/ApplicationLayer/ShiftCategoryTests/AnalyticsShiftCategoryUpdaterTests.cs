using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategory;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftCategoryTests
{
	[TestFixture]
	public class AnalyticsShiftCategoryUpdaterTests
	{
		private AnalyticsShiftCategoryUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private FakeShiftCategoryRepository _shiftCategoryRepository;
		private FakeAnalyticsShiftCategoryRepository _analyticsShiftCategoryRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_shiftCategoryRepository = new FakeShiftCategoryRepository();
			_analyticsShiftCategoryRepository = new FakeAnalyticsShiftCategoryRepository();
			_target = new AnalyticsShiftCategoryUpdater(_shiftCategoryRepository, _analyticsShiftCategoryRepository, _analyticsBusinessUnitRepository);
		}

		[Test]
		public void ShouldAddShiftCategoryToAnalytics()
		{
			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1", "red");
			shiftCategory.SetId(Guid.NewGuid());
			_shiftCategoryRepository.Add(shiftCategory);
			_target.Handle(new ShiftCategoryChangedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsShiftCategory = _analyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsShiftCategory.ShiftCategoryCode.Should().Be.EqualTo(shiftCategory.Id.GetValueOrDefault());
			analyticsShiftCategory.DisplayColor.Should().Be.EqualTo(shiftCategory.DisplayColor.ToArgb());
			analyticsShiftCategory.ShiftCategoryName.Should().Be.EqualTo(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldUpdateShiftCategoryToAnalytics()
		{
			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("New Shift Category Name", "red");
			shiftCategory.SetId(Guid.NewGuid());
			_shiftCategoryRepository.Add(shiftCategory);
			_analyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategory.Id.GetValueOrDefault(),
				ShiftCategoryName = "Old Shift Category Name",
				DisplayColor = 123
			});
			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);

			_target.Handle(new ShiftCategoryChangedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsScenario = _analyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsScenario.ShiftCategoryName.Should().Be.EqualTo("New Shift Category Name");
			analyticsScenario.DisplayColor.Should().Be.EqualTo(Color.FromName("red").ToArgb());
		}

		[Test]
		public void ShouldSetShiftCategoryToDelete()
		{
			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1", "red");
			shiftCategory.SetId(Guid.NewGuid());
			_shiftCategoryRepository.Add(shiftCategory);
			_analyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategory.Id.GetValueOrDefault()
			});
			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			_analyticsShiftCategoryRepository.ShiftCategories().First().IsDeleted.Should().Be.False();

			_target.Handle(new ShiftCategoryDeletedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.GetValueOrDefault()
			});

			_analyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsShiftCategory = _analyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsShiftCategory.IsDeleted.Should().Be.True();
		}
	}
}