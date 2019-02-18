using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategoryHandlers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftCategoryHandlers
{
	[DomainTest]
	public class ShiftCategorySelectionUpdaterTest : IIsolateSystem
	{
		public ShiftCategorySelectionModelUpdater Target;
		public FakeShiftCategorySelectionRepository ShiftCategorySelectionRepository;
		public FakeShiftCategoryUsageFinder ShiftCategoryUsageFinder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldCreateModelWhenNoPreviousExists()
		{
			BusinessUnitRepository.Add(BusinessUnitUsedInTests.BusinessUnit);

			Target.Handle(new TenantDayTickEvent());

			ShiftCategorySelectionRepository.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldUpdateModelWhenPreviousExists()
		{
			BusinessUnitRepository.Add(BusinessUnitUsedInTests.BusinessUnit);
			ShiftCategoryUsageFinder.Has(new ShiftCategoryExample
			{
				DayOfWeek = DayOfWeek.Wednesday,
				StartTime = 8.0,
				EndTime = 17.0,
				ShiftCategory = Guid.NewGuid().ToString()
			});
			ShiftCategorySelectionRepository.Add(new ShiftCategorySelection{Model = ""});

			Target.Handle(new TenantDayTickEvent());

			ShiftCategorySelectionRepository.LoadAll().First().Model.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldCreateModelWhenNoPreviousExistsOnShiftCategoryDeleted()
		{
			Target.Handle(new ShiftCategoryDeletedEvent());

			ShiftCategorySelectionRepository.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldUpdateModelWhenPreviousExistsOnShiftCategoryDeleted()
		{
			ShiftCategoryUsageFinder.Has(new ShiftCategoryExample
			{
				DayOfWeek = DayOfWeek.Wednesday,
				StartTime = 8.0,
				EndTime = 17.0,
				ShiftCategory = Guid.NewGuid().ToString()
			});
			ShiftCategorySelectionRepository.Add(new ShiftCategorySelection { Model = "" });

			Target.Handle(new ShiftCategoryDeletedEvent());

			ShiftCategorySelectionRepository.LoadAll().First().Model.Should().Not.Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ShiftCategorySelectionModelUpdater>().For<ShiftCategorySelectionModelUpdater>();
		}
	}
}
