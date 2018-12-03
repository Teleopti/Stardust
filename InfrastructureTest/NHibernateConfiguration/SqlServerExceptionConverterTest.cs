using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	[Category("BucketB")]
	public class SqlServerExceptionConverterTest : DatabaseTest
	{
		[Test]
		public void VerifyDuplicateKey()
		{
			CleanUpAfterTest();
			var ok = false;
			var per1 = PersonFactory.CreatePerson();
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
		public void VerifyViolatingUniqueIndex()
		{
			var name = RandomName.Make();
			var settingData1 = new GlobalSettingData(name);
			var settingData2 = new GlobalSettingData(name);
			PersistAndRemoveFromUnitOfWork(settingData1);
			Assert.Throws<ConstraintViolationException>(() =>
				PersistAndRemoveFromUnitOfWork(settingData2));
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

		[Test, Ignore("Retry strategy is applied now")]
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
			UnitOfWork.FetchSession().Connection.Close();
			Assert.Throws<DataSourceException>(() => UnitOfWork.PersistAll());				
		}

		[Test]
		public void DataSourceExceptionShouldNotBeThrownIfZombiedTransactionWhenDisposing()
		{
			UnitOfWork.FetchSession().Connection.Close();
			UnitOfWork.Dispose(); //implicit rollback
		}

		[Test, Ignore("Retry strategy is applied now")]
		public void DataSourceExceptionShouldBeThrownIfDeadConnectionWhenTransactionStarted()
		{
			var tempUow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			grabSessionFieldFromUow(tempUow).Connection.Close();
			Assert.Throws<CouldNotCreateTransactionException>(() =>
			                        new PersonRepository(new ThisUnitOfWork(tempUow)).Get(Guid.NewGuid()));

		}

		private ISession grabSessionFieldFromUow(IUnitOfWork uow)
		{
			return (ISession)UnitOfWork.GetType()
				.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(uow);
		}
	}
}