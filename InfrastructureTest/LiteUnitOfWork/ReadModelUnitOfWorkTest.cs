using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Autofac;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterType<TheService>().ApplyAspects();
			builder.RegisterType<NestedService>().AsSelf().As<NestedBase>().SingleInstance();
			builder.RegisterType<NestedService2>().AsSelf().As<NestedBase>().SingleInstance();

			builder.RegisterInstance(new MutableFakeCurrentHttpContext()).AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterInstance(new MutableFakeCurrentTeleoptiPrincipal()).AsSelf().As<ICurrentTeleoptiPrincipal>().SingleInstance();

			builder.Register(c =>
			{
				var dataSourcesProvider = new FakeCurrentApplicationData();
				var dataSource = c.Resolve<IDataSourcesFactory>().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null);
				dataSourcesProvider.RegisteredDataSourceCollection = new List<IDataSource>{dataSource};
				return dataSourcesProvider;
			}).AsSelf().As<ICurrentApplicationData>().SingleInstance();

		}
	}

	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class ReadModelUnitOfWorkTest
	{
		public TheService TheService;
		public NestedService NestedService;
		public NestedService2 NestedService2;
		public MutableFakeCurrentHttpContext HttpContext;
		public ICurrentReadModelUnitOfWork UnitOfWork;
		public MutableFakeCurrentTeleoptiPrincipal Principal;
		public IDataSourcesFactory DataSourcesFactory;
		public FakeCurrentApplicationData ApplicationData;
		public FakeConfigReader ConfigReader;

		[Test]
		[TestTable("TestTable")]
		public void ShouldProduceWorkingUnitOfWork()
		{
			var value = new Random().Next(-10000, -2).ToString();

			TheService.DoesUpdate(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value));

			TestTable.Values("TestTable").Single().Should().Be(value);
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldRollbackTransaction()
		{
			Assert.Throws<TestException>(() =>
			{
				TheService.Does(uow =>
				{
					uow.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
					throw new TestException();
				});
			});

			TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldProduceUnitOfWorkToNestedServices()
		{
			string result = null;
			NestedService.Action = uow => { result = uow.CreateSqlQuery("SELECT @@VERSION").List<string>().Single(); };

			TheService.CallNestedServices();

			result.Should().Contain("SQL");
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldRollbackTransactionForNestedServices()
		{
			NestedService.Action = s =>
			{
				s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
				throw new TestException();
			};

			Assert.Throws<TestException>(TheService.CallNestedServices);

			TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldSpanTransactionOverAllNestedServices()
		{
			NestedService.Action = s => s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
			NestedService2.Action = s =>
			{
				s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (2)").ExecuteUpdate();
				throw new TestException();
			};
			Assert.Throws<TestException>(TheService.CallNestedServices);

			TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
		}

		[Test]
		[TestTable("TestTable1")]
		[TestTable("TestTable2")]
		public void ShouldProduceUnitOfWorkForEachThread()
		{
			var thread1 = onAnotherThread(() =>
			{
				TheService.Does(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
			});
			var thread2 = onAnotherThread(() =>
			{
				TheService.Does(uow =>
				{
					1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable2 (Value) VALUES (0)").ExecuteUpdate());
					throw new TestException();
				});
			});
			thread1.Join();
			thread2.Join();

			TestTable.Values("TestTable1").Count().Should().Be(1000);
			TestTable.Values("TestTable2").Count().Should().Be(0);
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldProduceUnitOfWorkForWebRequestSpanning2Threads()
		{
			HttpContext.SetContext(new FakeHttpContext());
			TheService.Does(uow =>
			{
				onAnotherThread(() =>
				{
					UnitOfWork.Current().CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
				}).Join();
			});

			TestTable.Values("TestTable").Count().Should().Be(1);
		}

		[Test]
		[TestTable("TestTable1")]
		[TestTable("TestTable2")]
		public void ShouldProduceUnitOfWorkForEachWebRequest()
		{
			var thread1 = onAnotherThread(() =>
			{
				HttpContext.SetContextOnThread(new FakeHttpContext());
				TheService.Does(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
			});
			var thread2 = onAnotherThread(() =>
			{
				HttpContext.SetContextOnThread(new FakeHttpContext());
				TheService.Does(uow =>
				{
					1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable2 (Value) VALUES (0)").ExecuteUpdate());
					throw new TestException();
				});
			});
			thread1.Join();
			thread2.Join();

			TestTable.Values("TestTable1").Count().Should().Be(1000);
			TestTable.Values("TestTable2").Count().Should().Be(0);
		}

		[Test]
		public void ShouldReturnNullWhenNoCurrentUnitOfWork()
		{
			var target = TheService;

			target.Does(uow => { });

			UnitOfWork.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldProduceUnitOfWorkForEachDataSourceOnPrincipal()
		{
			using (new TestTable("TestTable", ConnectionStringHelper.ConnectionStringUsedInTests))
			using (new TestTable("TestTable", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			{
				var factory = DataSourcesFactory;
				var dataSource1 = factory.Create("One", ConnectionStringHelper.ConnectionStringUsedInTests, null);
				var dataSource2 = factory.Create("Two", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, null);

				Principal.SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource1, null, null), null));
				TheService.DoesUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				Principal.SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource2, null, null), null));
				TheService.DoesUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTests).Count().Should().Be(1);
				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix).Count().Should().Be(1);
			}
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldProduceUnitOfWorkForDataSourceMatchingRtaConnectionStringIfNoPrincipal()
		{
			var rtaConnectionString = new SqlConnectionStringBuilder(ConnectionStringHelper.ConnectionStringUsedInTests).ConnectionString;
			rtaConnectionString.Should().Not.Be.EqualTo(ConnectionStringHelper.ConnectionStringUsedInTests);
			ConfigReader.ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("Wrong", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix),
					new ConnectionStringSettings("RtaApplication", rtaConnectionString)
				};

			var factory = DataSourcesFactory;
			var dataSource1 = factory.Create("Wrong", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, null);
			var dataSource2 = factory.Create("Correct", ConnectionStringHelper.ConnectionStringUsedInTests, null);
			ApplicationData.RegisteredDataSourceCollection = new[] { dataSource1, dataSource2 }.Randomize();

			Principal.SetPrincipal(null);

			TheService.DoesUpdate("INSERT INTO TestTable (Value) VALUES (0)");

			TestTable.Values("TestTable").Count().Should().Be(1);
		}

		[Test]
		public void ShouldExecuteCodeAfterSuccessfulCommit()
		{
			string result = null;
			NestedService.ActionWSync = (uow, sync) => sync.OnSuccessfulTransaction(() => result = "success");

			TheService.CallNestedServices();

			result.Should().Be("success");
		}

		[Test]
		public void ShouldNotExecuteCodeAfterFailedCommit()
		{
			var result = "failed";
			NestedService.ActionWSync = (uow, sync) =>
			{
				sync.OnSuccessfulTransaction(() => result = "success");
				throw new TestException();
			};

			Assert.Throws<TestException>(TheService.CallNestedServices);

			result.Should().Be("failed");
		}

		private static Thread onAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}

	}

	public static class Extensions
	{
		public static void DoesUpdate(this TheService instance, string query)
		{
			instance.Does(uow => uow.CreateSqlQuery(query).ExecuteUpdate());
		}

		public static T MakesQuery<T>(this TheService instance, string query, Func<ISQLQuery, T> queryAction)
		{
			var result = default(T);
			instance.Does(uow =>
			{
				result = queryAction(uow.CreateSqlQuery(query));
			});
			return result;
		}
	}

	public class TestException : Exception
	{
	}

	public class TheService
	{
		private readonly IEnumerable<NestedBase> _nested;
		private readonly ICurrentReadModelUnitOfWork _uow;

		public TheService(IEnumerable<NestedBase> nested, ICurrentReadModelUnitOfWork uow)
		{
			_nested = nested;
			_uow = uow;
		}

		[ReadModelUnitOfWork]
		public virtual void Does(Action<ILiteUnitOfWork> action)
		{
			action(_uow.Current());
		}

		[ReadModelUnitOfWork]
		public virtual void CallNestedServices()
		{
			_nested.ForEach(i => i.ExecuteAction());
		}
	}

	public class NestedService : NestedBase
	{
		public NestedService(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class NestedService2 : NestedBase
	{
		public NestedService2(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class NestedBase
	{
		private readonly ICurrentReadModelUnitOfWork _uow;
		private readonly ILiteTransactionSyncronization _syncronization;

		public NestedBase(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
		{
			_uow = uow;
			_syncronization = syncronization;
		}

		public Action<ILiteUnitOfWork> Action = u => { };
		public Action<ILiteUnitOfWork, ILiteTransactionSyncronization> ActionWSync = (u, s) => { };

		public void ExecuteAction()
		{
			Action.Invoke(_uow.Current());
			ActionWSync.Invoke(_uow.Current(), _syncronization);
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class TestTableAttribute : Attribute, ITestAction
	{
		private readonly string _name;
		private TestTable _table;

		public TestTableAttribute(string name)
		{
			_name = name;
		}

		public ActionTargets Targets { get { return ActionTargets.Test; } }

		public void BeforeTest(TestDetails testDetails)
		{
			_table = new TestTable(_name);
		}

		public void AfterTest(TestDetails testDetails)
		{
			_table.Dispose();
			_table = null;
		}

	}

	public class TestTable : IDisposable
	{
		private readonly string _name;
		private readonly string _connectionString;

		public TestTable(string name)
			: this(name, ConnectionStringHelper.ConnectionStringUsedInTests)
		{
		}

		public TestTable(string name, string connectionString)
		{
			_name = name;
			_connectionString = connectionString;
			applySql(string.Format("CREATE TABLE {0} (Value int)", _name));
		}

		public static IEnumerable<int> Values(string tableName)
		{
			return Values(tableName, ConnectionStringHelper.ConnectionStringUsedInTests);
		}

		public static IEnumerable<int> Values(string tableName, string connectionString)
		{
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("SELECT * FROM " + tableName, connection))
				using (var reader = command.ExecuteReader())
					while (reader.Read())
						yield return reader.GetInt32(0);
			}
		}

		public void Dispose()
		{
			applySql(string.Format("DROP TABLE {0}", _name));
		}

		private void applySql(string sql)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(sql, connection))
					command.ExecuteNonQuery();
			}
		}
	}
}