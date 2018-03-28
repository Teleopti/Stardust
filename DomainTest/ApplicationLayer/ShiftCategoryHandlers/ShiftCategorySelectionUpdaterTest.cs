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
	public class ShiftCategorySelectionUpdaterTest
	{
		public ShiftCategorySelectionModelUpdater Target;
		public FakeShiftCategorySelectionRepository ShiftCategorySelectionRepository;
		public FakeShiftCategoryUsageFinder ShiftCategoryUsageFinder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldCreateModelWhenNoPreviousExists()
		{
			BusinessUnitRepository.Add(BusinessUnitFactory.BusinessUnitUsedInTest);

			Target.Handle(new TenantDayTickEvent());

			ShiftCategorySelectionRepository.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldUpdateModelWhenPreviousExists()
		{
			BusinessUnitRepository.Add(BusinessUnitFactory.BusinessUnitUsedInTest);
			ShiftCategoryUsageFinder.Has(new testModel
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
			ShiftCategoryUsageFinder.Has(new testModel
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

		private class testModel : IShiftCategoryPredictorModel
		{
			public double StartTime { get; set; }
			public double EndTime { get; set; }
			public DayOfWeek DayOfWeek { get; set; }
			public string ShiftCategory { get; set; }
		}
	}
}
