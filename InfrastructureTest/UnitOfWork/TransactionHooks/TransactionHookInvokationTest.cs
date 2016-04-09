using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[DatabaseTest]
	public class TransactionHookInvokationTest
	{
		public FakeTransactionHook TransactionHook;
		public IPersonRepository Persons;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldInvoke()
		{
			WithUnitOfWork.Do(() =>
			{
				Persons.Add(PersonFactory.CreatePerson());
			});

			TransactionHook.ModifiedRoots.OfType<IPerson>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldThrowIfFails()
		{
			TransactionHook.Fails();

			Assert.Throws<DataSourceException>(() =>
			{
				WithUnitOfWork.Do(() =>
				{
					Persons.Add(PersonFactory.CreatePerson());
				});
			});
		}
	}
}