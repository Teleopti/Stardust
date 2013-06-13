#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Teleopti.Messaging.Composites;
using Teleopti.Messaging.SignalR;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.InfrastructureTest
{
	/// <summary>
	/// Setup fixture for assembly
	/// </summary>
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		private MockRepository mocks;
		private IState stateMock;
		internal static IPerson loggedOnPerson;
		internal static IApplicationData ApplicationData;
		private static ISessionData sessionData;
		internal static IDataSource DataSource;

		/// <summary>
		/// Runs before any test.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), SetUp]
		public void RunBeforeAnyTest()
		{
			mocks = new MockRepository();
			stateMock = mocks.StrictMock<IState>();

			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				 name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			DataSource = DataSourceHelper.CreateDataSource();

			loggedOnPerson = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatClenUpDataSource", string.Empty);

			ApplicationData = new ApplicationData(appSettings,
									new ReadOnlyCollection<IDataSource>(new List<IDataSource> { DataSource }),
									new SignalBroker(MessageFilterManager.Instance.FilterDictionary), null);
			sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, ApplicationData, BusinessUnitFactory.BusinessUnitUsedInTest);

			StateHolderProxyHelper.SetStateReaderExpectations(stateMock, ApplicationData, sessionData);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

			mocks.ReplayAll();

			deleteEverythingInDb();
			configureLog4net();
		}

		[TearDown]
		public void RunAfterTestSuite()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				CleanupHistory(uow);
				uow.PersistAll();
			}
			checkThatDbIsEmtpy();

			DataSource.Application.Close();
			if (DataSource.Statistic != null)
				DataSource.Statistic.Close();	
		}

		public static void CleanupHistory(IUnitOfWork uow)
		{
			var s = session(uow);
			foreach (var classMetaData in s.SessionFactory.GetAllClassMetadata().Values)
			{
				var entityName = classMetaData.EntityName;
				if (entityName.Contains("_AUD"))
					s.CreateQuery("delete from " + entityName).ExecuteUpdate();
			}
			s.CreateQuery("delete from Revision").ExecuteUpdate();
		}

		private static void configureLog4net()
		{
			if (IniFileInfo.SqlOutput)
			{
				var app = new TraceAppender();
				app.Threshold = Level.Warn;
				app.Layout = new SimpleLayout();
				BasicConfigurator.Configure(app);
			}
		}

		private static void checkThatDbIsEmtpy()
		{
			const string assertMess =
				 @"After running last test in suite, there's still data in db.
Every test that's marked as 'SkipRollBack()' have to clean up db rows.
This hasn't been done somewhere. Unfortunatly you will have to search yourself 
in what infrastructuretest this has happened - it is unknown for me.";

			createTemporaryStateHolder();

			using (IUnitOfWork uowTemp = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				using (uowTemp.DisableFilter(QueryFilter.BusinessUnit))
				{
					ISession s = session(uowTemp);
					s.CreateSQLQuery(@"delete from PersonWriteProtectionInfo").ExecuteUpdate();
					IList<IAggregateRoot> leftInDb = s.CreateCriteria(typeof(IAggregateRoot))
											  .List<IAggregateRoot>();
					if (leftInDb.Count > 0)
					{
						string mess = string.Concat(assertMess, "\n\nThe problem is with following roots...");
						leftInDb.ForEach(root => mess = string.Concat(mess, "\n", root.GetType(), " : ", root.Id));
						Assert.Fail(mess);
					}
				}
			}
		}

		private static ISession session(IUnitOfWork uowTemp)
		{
			return (ISession)uowTemp.GetType().GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uowTemp, null);
		}

		/// <summary>
		/// Creates a local temporary state holder.
		/// </summary>
		/// <remarks>
		/// we need to rectreated a local temporary mocked stateholder to make the ApplicationData object available in the stateholder when creting
		/// Unit of work in DB empt check. It is because we are in the verified state of the old mock object, so its properies are not available
		/// but we want to read its ApplicationScopeData property
		/// </remarks>
		private static void createTemporaryStateHolder()
		{
			var mocks = new MockRepository();
			var stateMock = mocks.StrictMock<IState>();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(Guid.NewGuid());
			StateHolderProxyHelper.ClearAndSetStateHolder(mocks,
																  loggedOnPerson,
																  BusinessUnitFactory.BusinessUnitUsedInTest,
																  ApplicationData,
																  stateMock);
		}

		private static void deleteEverythingInDb()
		{
			//needed if db isn't created by nh but by script:s that also have inserts
			//IPerson per = fakeLogon();

			using (IUnitOfWork uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{

				ISession session = (ISession)uow.GetType()
					 .GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uow, null);

				IRepository rep = new Repository(uow);

				rep.Add(loggedOnPerson);

				//force a insert
				BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
				session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, Guid.NewGuid());
				uow.PersistAll();

				var allDbRoots = session.CreateCriteria(typeof(IAggregateRoot))
					 .List<IAggregateRoot>();
				foreach (var aggregateRoot in allDbRoots)
				{
					if (!(aggregateRoot is IPersonWriteProtectionInfo))
						rep.Remove(aggregateRoot);
				}

				uow.PersistAll();
			}
		}

		internal static IDictionary<string, string> Sql2005conf(string connString, int? timeout)
		{
			return DataSourceHelper.CreateDataSourceSettings(connString, timeout);
		}
	}
}
