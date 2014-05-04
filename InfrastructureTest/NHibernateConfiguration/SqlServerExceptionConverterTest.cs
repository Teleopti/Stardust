using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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
			SkipRollback();
			var ok = false;
			var per1 = new Person();
			addLogonInfo(per1);
			var per2 = new Person();
			addLogonInfo(per2);
			try
			{
				IPersonRepository rep = new PersonRepository(UnitOfWork);
				rep.Add(per1);
				rep.Add(per2);
				UnitOfWork.PersistAll();
			}
			catch (ConstraintViolationException)
			{
				ok = true;
			}
			if (!ok)
				Assert.Fail("ConstraintViolationException was not thrown!");
		}


		private static void addLogonInfo(IPerson person)
		{
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
		    person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
		                                               {ApplicationLogOnName = "a", Password = "b"};
		}

		[Test]
		public void GeneralException()
		{
			var ok = false;
			try
			{
				Session.CreateSQLQuery("select foo from bar").List();
			}
			
			catch(Exception)
			{
				ok = true;
			}
			if (!ok)
				Assert.Fail("Exception was not thrown!");
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
			Assert.Throws<DataSourceException>(() =>
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