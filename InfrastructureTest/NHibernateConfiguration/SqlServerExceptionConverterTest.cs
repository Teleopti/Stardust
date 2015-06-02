using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	[Category("LongRunning")]
	public class SqlServerExceptionConverterTest : DatabaseTest
	{
		[Test]
		public void VerifyViolatingUniqueIndex()
		{
			CleanUpAfterTest();
			var ok = false;
			var per1 = new Person();
			var scenario = new Scenario("dsf");
			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(scenario);
			var ass1 = new PersonAssignment(per1, scenario, new DateOnly(2000,1,1));
			var ass2 = new PersonAssignment(per1, scenario, new DateOnly(2000,1,1));
			try
			{
				var rep = new PersonAssignmentRepository(UnitOfWork);
				rep.Add(ass1);
				rep.Add(ass2);
				UnitOfWork.PersistAll();
			}
			catch (ConstraintViolationException)
			{
				ok = true;
			}
			if (!ok)
				Assert.Fail("ConstraintViolationException was not thrown!");
		}

		[Test]
		public void GeneralException()
		{
			var ok = false;
			try
			{
				Session.CreateSQLQuery("select foo from bar").List();
			}
			catch (DataSourceException)
			{
				ok = true;
			}
			if (!ok)
				Assert.Fail("DataSourceException was not thrown!");
		}

		[Test]
		public void DataSourceExceptionShouldBeThrownIfNotConnectedWhenQueryIsStarted()
		{
			using (var tempSession = Session.SessionFactory.OpenSession())
			{
				tempSession.Connection.Close();
				Assert.Throws<DataSourceException>(() =>
					tempSession.CreateSQLQuery("select 1").List());
			}
		}

		[Test]
		public void DataSourceExceptionShouldBeThrownIfZombiedTransactionWhenCommit()
		{
			var tempUow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			tempUow.FetchSession().Connection.Close();
			Assert.Throws<DataSourceException>(() => tempUow.PersistAll());				
		}

		[Test]
		public void DataSourceExceptionShouldNotBeThrownIfZombiedTransactionWhenDisposing()
		{
			var tempUow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			tempUow.FetchSession().Connection.Close();
			tempUow.Dispose(); //implicit rollback
		}

		[Test]
		public void DataSourceExceptionShouldBeThrownIfDeadConnectionWhenTransactionStarted()
		{
			var tempUow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			grabSessionFieldFromUow(tempUow).Connection.Close();
			Assert.Throws<CouldNotCreateTransactionException>(() =>
			                        new PersonRepository(tempUow).Get(Guid.NewGuid()));

		}

		private ISession grabSessionFieldFromUow(IUnitOfWork uow)
		{
			return (ISession)UnitOfWork.GetType()
				.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(uow);
		}
	}
}