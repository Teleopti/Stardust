using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class PersonAbsenceAccountConflictCollectorTest
	{
		private MockRepository _mocks;
		private PersonAbsenceAccountConflictCollector _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new PersonAbsenceAccountConflictCollector();
		}

		[Test]
		public void ShouldReturnItemsWithDifferenceInDatabaseVersionOnGetConflicts()
		{
			var nonConflictingEntity = _mocks.Stub<IPersonAbsenceAccount>();
			Expect.Call(nonConflictingEntity.Version).Return(1).Repeat.Any();
			var conflictingEntity = _mocks.Stub<IPersonAbsenceAccount>();
			Expect.Call(conflictingEntity.Version).Return(1).Repeat.Any();
			var entities = new[] {nonConflictingEntity, conflictingEntity};

			var unitOfWork = _mocks.StrictMock<IUnitOfWork>();

			Expect.Call(unitOfWork.DatabaseVersion(nonConflictingEntity)).Return(1);
			Expect.Call(unitOfWork.DatabaseVersion(conflictingEntity)).Return(2);

			_mocks.ReplayAll();

			var conflicts = _target.GetConflicts(unitOfWork, entities);

			_mocks.VerifyAll();

			Assert.That(conflicts.Single(), Is.SameAs(conflictingEntity));
		}
	}
}