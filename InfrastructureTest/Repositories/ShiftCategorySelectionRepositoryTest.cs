using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class ShiftCategorySelectionRepositoryTest : RepositoryTest<IShiftCategorySelection>
	{
		protected override IShiftCategorySelection CreateAggregateWithCorrectBusinessUnit()
		{
			return new ShiftCategorySelection {Model = "test model"};
		}

		protected override void VerifyAggregateGraphProperties(IShiftCategorySelection loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Id.HasValue.Should().Be.True();
			loadedAggregateFromDatabase.Model.Should().Be.EqualTo("test model");
		}

		protected override Repository<IShiftCategorySelection> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ShiftCategorySelectionRepository(currentUnitOfWork);
		}
	}
}