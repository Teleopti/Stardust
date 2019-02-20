using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftCategoryHandlers
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsShiftCategoryUpdaterTests : IExtendSystem
	{
		public AnalyticsShiftCategoryUpdater Target;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeAnalyticsShiftCategoryRepository AnalyticsShiftCategoryRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsShiftCategoryUpdater>();
		}

		[Test]
		public void ShouldAddShiftCategoryToAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1", "red");
			shiftCategory.SetId(Guid.NewGuid());
			ShiftCategoryRepository.Add(shiftCategory);
			Target.Handle(new ShiftCategoryChangedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsShiftCategory = AnalyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsShiftCategory.ShiftCategoryCode.Should().Be.EqualTo(shiftCategory.Id.GetValueOrDefault());
			analyticsShiftCategory.DisplayColor.Should().Be.EqualTo(shiftCategory.DisplayColor.ToArgb());
			analyticsShiftCategory.ShiftCategoryName.Should().Be.EqualTo(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldUpdateShiftCategoryToAnalytics()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("New Shift Category Name", "red");
			shiftCategory.SetId(Guid.NewGuid());
			ShiftCategoryRepository.Add(shiftCategory);
			AnalyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategory.Id.GetValueOrDefault(),
				ShiftCategoryName = "Old Shift Category Name",
				DisplayColor = 123
			});
			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);

			Target.Handle(new ShiftCategoryChangedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsScenario = AnalyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsScenario.ShiftCategoryName.Should().Be.EqualTo("New Shift Category Name");
			analyticsScenario.DisplayColor.Should().Be.EqualTo(Color.FromName("red").ToArgb());
		}

		[Test]
		public void ShouldSetShiftCategoryToDelete()
		{
			BusinessUnitRepository.Has(BusinessUnitUsedInTests.BusinessUnit);
			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(0);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory1", "red");
			shiftCategory.SetId(Guid.NewGuid());
			ShiftCategoryRepository.Add(shiftCategory);
			AnalyticsShiftCategoryRepository.AddShiftCategory(new AnalyticsShiftCategory
			{
				ShiftCategoryCode = shiftCategory.Id.GetValueOrDefault()
			});
			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			AnalyticsShiftCategoryRepository.ShiftCategories().First().IsDeleted.Should().Be.False();

			Target.Handle(new ShiftCategoryDeletedEvent
			{
				ShiftCategoryId = shiftCategory.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = BusinessUnitUsedInTests.BusinessUnit.Id.GetValueOrDefault()
			});

			AnalyticsShiftCategoryRepository.ShiftCategories().Count.Should().Be.EqualTo(1);
			var analyticsShiftCategory = AnalyticsShiftCategoryRepository.ShiftCategories().First();
			analyticsShiftCategory.IsDeleted.Should().Be.True();
		}
	}
}