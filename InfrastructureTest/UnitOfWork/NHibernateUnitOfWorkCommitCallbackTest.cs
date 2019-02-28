using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class NHibernateUnitOfWorkCommitCallbackTest : DatabaseTest
	{
		[Test]
		public void ShouldCallTxCallbackWhenSuccessfulCommit()
		{
			CleanUpAfterTest();
			var isCalled = false;
			UnitOfWork.AfterSuccessfulTx(() => isCalled = true);
			UnitOfWork.PersistAll();
			isCalled.Should().Be.True();
		}

		[Test]
		public void ShouldNotCallTxCallbackWhenRollbacked()
		{
			var isCalled = false;
			UnitOfWork.AfterSuccessfulTx(() => isCalled = true);
			isCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotCallTxCallbackIfException()
		{
			CleanUpAfterTest();
			var isCalled = false;
			var person = new Person().WithId();
			var correctEx = false;

			UnitOfWork.AfterSuccessfulTx(() => isCalled = true);
			try
			{
				PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(UnitOfWork), null, null).Add(person);
				UnitOfWork.PersistAll();
			}
			catch (DataSourceException)
			{
				correctEx = true;
			}

			correctEx.Should().Be.True();
			isCalled.Should().Be.False();
		}

		[Test]
		public void ShouldWorkWithCurrentUnitOfWork()
		{
			CleanUpAfterTest();
			var isCalled = false;
			UnitOfWorkFactory.Current.CurrentUnitOfWork().AfterSuccessfulTx(() => isCalled = true);
			UnitOfWorkFactory.Current.CurrentUnitOfWork().PersistAll();
			isCalled.Should().Be.True();
		}
	}
}