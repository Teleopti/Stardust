using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftCategory;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftCategory
{
	[DomainTest]
	public class ShiftCategorySelectionTest
	{
		public ShiftCategorySelectionModelUpdater Target;
		public FakeShiftCategorySelectionRepository ShiftCategorySelectionRepository;

		[Test]
		public void ShouldCreateModelWhenNoPreviousExists()
		{
			Target.Handle(new TenantDayTickEvent());

			ShiftCategorySelectionRepository.LoadAll().Should().Not.Be.Empty();
		}
	}
}
