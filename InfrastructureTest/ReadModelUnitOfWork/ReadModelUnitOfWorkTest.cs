using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.ReadModelUnitOfWork
{
	public class ReadModelUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddService<TheService>();
			system.AddService<NestedService1>();
			system.AddService<NestedService2>();

			system.UseTestDouble(new MutableFakeCurrentHttpContext()).For<ICurrentHttpContext>();
			system.UseTestDouble<FakeCurrentTeleoptiPrincipal>().For<ICurrentTeleoptiPrincipal>();
		}
	}

	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class ReadModelUnitOfWorkTest
	{
		public TheService TheService;
		public NestedService1 NestedService1;
		public NestedService2 NestedService2;
		public MutableFakeCurrentHttpContext HttpContext;
		public ICurrentReadModelUnitOfWork UnitOfWork;
		public FakeCurrentTeleoptiPrincipal Principal;
		public IDataSourcesFactory DataSourcesFactory;
		public FakeConfigReader ConfigReader;
		public IDataSourceScope DataSource;

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
			NestedService1.Action = uow => { result = uow.CreateSqlQuery("SELECT @@VERSION").List<string>().Single(); };

			TheService.CallNestedServices();

			result.Should().Contain("SQL");
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldRollbackTransactionForNestedServices()
		{
			NestedService1.Action = s =>
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
			NestedService1.Action = s => s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
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
			using (new TestTable("TestTable", InfraTestConfigReader.ConnectionString))
			using (new TestTable("TestTable", InfraTestConfigReader.AnalyticsConnectionString))
			{
				var factory = DataSourcesFactory;
				var dataSource1 = factory.Create("One", InfraTestConfigReader.ConnectionString, null);
				var dataSource2 = factory.Create("Two", InfraTestConfigReader.AnalyticsConnectionString, null);

				Principal.Fake(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource1, null, null, null), null));
				TheService.DoesUpdateWithoutDatasource("INSERT INTO TestTable (Value) VALUES (0)");

				Principal.Fake(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource2, null, null, null), null));
				TheService.DoesUpdateWithoutDatasource("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable", InfraTestConfigReader.ConnectionString).Count().Should().Be(1);
				TestTable.Values("TestTable", InfraTestConfigReader.AnalyticsConnectionString).Count().Should().Be(1);
			}
		}
		
		[Test]
		public void ShouldProduceUnitOfWorkForDataSourceOnThread()
		{
			using (new TestTable("TestTable1", InfraTestConfigReader.AnalyticsConnectionString))
			using (new TestTable("TestTable2", InfraTestConfigReader.ConnectionString))
			{
				var dataSource1 = DataSourcesFactory.Create("One", InfraTestConfigReader.AnalyticsConnectionString, null);
				var dataSource2 = DataSourcesFactory.Create("Two", InfraTestConfigReader.ConnectionString, null);

				var thread1 = onAnotherThread(() =>
				{
					using (DataSource.OnThisThreadUse(dataSource1))
					{
						TheService.DoesWithoutDatasource(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
					}
				});

				var thread2 = onAnotherThread(() =>
				{
					using (DataSource.OnThisThreadUse(dataSource2))
					{
						TheService.DoesWithoutDatasource(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable2 (Value) VALUES (0)").ExecuteUpdate()));
					}
				});

				thread1.Join();
				thread2.Join();

				TestTable.Values("TestTable1", InfraTestConfigReader.AnalyticsConnectionString).Count().Should().Be(1000);
				TestTable.Values("TestTable2", InfraTestConfigReader.ConnectionString).Count().Should().Be(1000);
			}
		}

		[Test]
		public void ShouldExecuteCodeAfterSuccessfulCommit()
		{
			string result = null;
			NestedService1.ActionWSync = (uow, sync) => sync.OnSuccessfulTransaction(() => result = "success");

			TheService.CallNestedServices();

			result.Should().Be("success");
		}

		[Test]
		public void ShouldNotExecuteCodeAfterFailedCommit()
		{
			var result = "failed";
			NestedService1.ActionWSync = (uow, sync) =>
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

		public static void DoesUpdateWithoutDatasource(this TheService instance, string query)
		{
			instance.DoesWithoutDatasource(uow => uow.CreateSqlQuery(query).ExecuteUpdate());
		}
	}

	public class TestException : Exception
	{
	}

	public class TheService
	{
		private readonly NestedService1 _nested1;
		private readonly NestedService2 _nested2;
		private readonly ICurrentReadModelUnitOfWork _uow;
		private readonly IDataSourceScope _dataSource;

		public TheService(
			NestedService1 nested1, 
			NestedService2 nested2, 
			ICurrentReadModelUnitOfWork uow, 
			IDataSourceScope dataSource)
		{
			_nested1 = nested1;
			_nested2 = nested2;
			_uow = uow;
			_dataSource = dataSource;
		}

		public virtual void Does(Action<ILiteUnitOfWork> action)
		{
			using (_dataSource.OnThisThreadUse("App"))
				DoesWithoutDatasource(action);
		}

		[ReadModelUnitOfWork]
		public virtual void DoesWithoutDatasource(Action<ILiteUnitOfWork> action)
		{
			action(_uow.Current());
		}

		public virtual void CallNestedServices()
		{
			using (_dataSource.OnThisThreadUse("App"))
				CallNestedServicesWithoutDatasource();
		}

		[ReadModelUnitOfWork]
		public virtual void CallNestedServicesWithoutDatasource()
		{
			_nested1.ExecuteAction();
			_nested2.ExecuteAction();
		}
	}

	public class NestedService1 : NestedBase
	{
		public NestedService1(ICurrentReadModelUnitOfWork uow, IReadModelTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class NestedService2 : NestedBase
	{
		public NestedService2(ICurrentReadModelUnitOfWork uow, IReadModelTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class NestedBase
	{
		private readonly ICurrentReadModelUnitOfWork _uow;
		private readonly IReadModelTransactionSyncronization _syncronization;

		public NestedBase(ICurrentReadModelUnitOfWork uow, IReadModelTransactionSyncronization syncronization)
		{
			_uow = uow;
			_syncronization = syncronization;
		}

		public Action<ILiteUnitOfWork> Action = u => { };
		public Action<ILiteUnitOfWork, IReadModelTransactionSyncronization> ActionWSync = (u, s) => { };

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
			: this(name, InfraTestConfigReader.ConnectionString)
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
			return Values(tableName, InfraTestConfigReader.ConnectionString);
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