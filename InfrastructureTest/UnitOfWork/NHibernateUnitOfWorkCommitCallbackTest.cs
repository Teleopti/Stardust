using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

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
			var person = new Person();
			var correctEx = false;

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			new PersonRepository(new ThisUnitOfWork(UnitOfWork)).Add(person);
			UnitOfWork.PersistAll();

			UnitOfWork.AfterSuccessfulTx(() => isCalled = true);
			//too long email
			var email = new string('c', 300);
			person.Email = email;
			try
			{
				UnitOfWork.PersistAll();
			}
			catch (DataSourceException)
			{
				correctEx = true;
			}

			using (var uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				person.Email = "ok";
				new PersonRepository(new ThisUnitOfWork(uow)).Remove(person);
				uow.PersistAll();
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