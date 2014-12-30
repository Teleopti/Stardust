using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Authentication;
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
	public class ReadModelUnitOfWorkTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<Outer>().EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterType<Inner1>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();
			builder.RegisterType<Inner2>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();

			var httpContext = new MutableFakeCurrentHttpContext();
			builder.RegisterInstance(httpContext).AsSelf().As<ICurrentHttpContext>().SingleInstance();

			builder.Register(c =>
			{
				var dataSourcesProvider = new FakeDataSourcesProvider();
				var dataSource = c.Resolve<IDataSourcesFactory>().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null);
				dataSourcesProvider.SetAvailableDataSources(new[] { dataSource });
				return dataSourcesProvider;
			}).AsSelf().As<IAvailableDataSourcesProvider>().SingleInstance();

			var currentPrincipal = new MutableFakeCurrentTeleoptiPrincipal();
			currentPrincipal.SetPrincipal(null);
			builder.RegisterInstance(currentPrincipal).AsSelf().As<ICurrentTeleoptiPrincipal>().SingleInstance();

			var configReader = new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
				}
			};
			builder.RegisterInstance(configReader).As<IConfigReader>().AsSelf().SingleInstance();
		}
	}

	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class ReadModelUnitOfWorkTest
	{
		public Outer Outer;
		public Inner1 Inner1;
		public Inner2 Inner2;
		public MutableFakeCurrentHttpContext HttpContext;
		public ICurrentReadModelUnitOfWork UnitOfWork;
		public MutableFakeCurrentTeleoptiPrincipal Principal;
		public IDataSourcesFactory DataSourcesFactory;
		public FakeDataSourcesProvider DataSourcesProvider;
		public FakeConfigReader ConfigReader;

		[Test]
		public void ShouldProduceWorkingUnitOfWork()
		{
			using (new TestTable("TestTable"))
			{
				var value = new Random().Next(-10000, -2).ToString();

				Outer.DoUpdate(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value));

				TestTable.Values("TestTable").Single().Should().Be(value);
			}
		}

		[Test]
		public void ShouldRollbackTransaction()
		{
			using (new TestTable("TestTable"))
			{
				Assert.Throws<TestException>(() =>
				{
					Outer.DoAction(uow =>
					{
						uow.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
						throw new TestException();
					});
				});

				TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkToInnerObjects()
		{
			string result = null;
			Inner1.Action = uow => { result = uow.CreateSqlQuery("SELECT @@VERSION").List<string>().Single(); };

			Outer.ExecuteInners();

			result.Should().Contain("SQL");
		}

		[Test]
		public void ShouldRollbackTransactionForInnerObject()
		{
			using (new TestTable("TestTable"))
			{
				Inner1.Action = s =>
				{
					s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
					throw new TestException();
				};

				Assert.Throws<TestException>(Outer.ExecuteInners);

				TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
			}
		}

		[Test]
		public void ShouldSpanTransactionOverAllInnerObjects()
		{
			using (new TestTable("TestTable"))
			{
				Inner1.Action = s => s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
				Inner2.Action = s =>
				{
					s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (2)").ExecuteUpdate();
					throw new TestException();
				};
				Assert.Throws<TestException>(Outer.ExecuteInners);

				TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForEachThread()
		{
			using (new TestTable("TestTable1"))
			using (new TestTable("TestTable2"))
			{
				var thread1 = onAnotherThread(() =>
				{
					Outer.DoAction(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
				});
				var thread2 = onAnotherThread(() =>
				{
					Outer.DoAction(uow =>
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
		}

		[Test]
		public void ShouldProduceUnitOfWorkForWebRequestSpanning2Threads()
		{
			using (new TestTable("TestTable"))
			{
				HttpContext.SetContext(new FakeHttpContext());
				Outer.DoAction(uow =>
				{
					onAnotherThread(() =>
					{
						UnitOfWork.Current().CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
					}).Join();
				});

				TestTable.Values("TestTable").Count().Should().Be(1);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForEachWebRequest()
		{
			using (new TestTable("TestTable1"))
			using (new TestTable("TestTable2"))
			{
				var thread1 = onAnotherThread(() =>
				{
					HttpContext.SetContextOnThread(new FakeHttpContext());
					Outer.DoAction(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
				});
				var thread2 = onAnotherThread(() =>
				{
					HttpContext.SetContextOnThread(new FakeHttpContext());
					Outer.DoAction(uow =>
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
		}

		[Test]
		public void ShouldReturnNullWhenNoCurrentUnitOfWork()
		{
			var target = Outer;

			target.DoAction(uow => { });

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
				Outer.DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				Principal.SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource2, null, null), null));
				Outer.DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTests).Count().Should().Be(1);
				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix).Count().Should().Be(1);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForDataSourceMatchingRtaConnectionStringIfNoPrincipal()
		{
			// ConfigurationManager.ConnectionStrings["RtaApplication"]
			using (new TestTable("TestTable"))
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
				DataSourcesProvider.SetAvailableDataSources(new[] { dataSource1, dataSource2 }.Randomize());

				Principal.SetPrincipal(null);

				Outer.DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable").Count().Should().Be(1);
			}
		}

		[Test]
		public void ShouldExecuteCodeAfterSuccessfulCommit()
		{
			string result = null;
			Inner1.ActionWSync = (uow, sync) => sync.OnSuccessfulTransaction(() => result = "success");

			Outer.ExecuteInners();

			result.Should().Be("success");
		}

		[Test]
		public void ShouldNotExecuteCodeAfterFailedCommit()
		{
			var result = "failed";
			Inner1.ActionWSync = (uow, sync) =>
			{
				sync.OnSuccessfulTransaction(() => result = "success");
				throw new TestException();
			};

			Assert.Throws<TestException>(Outer.ExecuteInners);

			result.Should().Be("failed");
		}

		private static Thread onAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}

	}

	public class FakeDataSourcesProvider : IAvailableDataSourcesProvider
	{
		private IEnumerable<IDataSource> _dataSources;

		public void SetAvailableDataSources(IEnumerable<IDataSource> dataSources)
		{
			_dataSources = dataSources;
		}

		public IEnumerable<IDataSource> AvailableDataSources()
		{
			return _dataSources;
		}

		public IEnumerable<IDataSource> UnavailableDataSources()
		{
			return null;
		}
	}

	public static class Extensions
	{
		public static void Times(this int times, Action<int> action)
		{
			Enumerable.Range(0, times).ForEach(action);
		}

		public static void DoUpdate(this Outer instance, string query)
		{
			instance.DoAction(uow => uow.CreateSqlQuery(query).ExecuteUpdate());
		}

		public static T DoSelect<T>(this Outer instance, string query, Func<ISQLQuery, T> queryAction)
		{
			var result = default(T);
			instance.DoAction(uow =>
			{
				result = queryAction(uow.CreateSqlQuery(query));
			});
			return result;
		}
	}

	public class TestException : Exception
	{
	}

	public class Outer
	{
		private readonly IEnumerable<ReadModelUnitOfWorkInnerTester> _inners;
		private readonly ICurrentReadModelUnitOfWork _uow;

		public Outer(IEnumerable<ReadModelUnitOfWorkInnerTester> inners, ICurrentReadModelUnitOfWork uow)
		{
			_inners = inners;
			_uow = uow;
		}

		[ReadModelUnitOfWork]
		public virtual void DoAction(Action<ILiteUnitOfWork> action)
		{
			action(_uow.Current());
		}

		[ReadModelUnitOfWork]
		public virtual void ExecuteInners()
		{
			_inners.ForEach(i => i.ExecuteAction());
		}
	}

	public class Inner1 : ReadModelUnitOfWorkInnerTester
	{
		public Inner1(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class Inner2 : ReadModelUnitOfWorkInnerTester
	{
		public Inner2(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
			: base(uow, syncronization)
		{
		}
	}

	public class ReadModelUnitOfWorkInnerTester
	{
		private readonly ICurrentReadModelUnitOfWork _uow;
		private readonly ILiteTransactionSyncronization _syncronization;

		public ReadModelUnitOfWorkInnerTester(ICurrentReadModelUnitOfWork uow, ILiteTransactionSyncronization syncronization)
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