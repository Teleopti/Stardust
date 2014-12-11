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
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	[TestFixture]
	public class ReadModelUnitOfWorkTest
	{
		[Test]
		public void ShouldProduceWorkingUnitOfWork()
		{
			using (new TestTable("TestTable"))
			using (var c = buildContainer())
			{
				var target = c.Resolve<Outer>();
				var value = new Random().Next(-10000, -2).ToString();

				target.DoUpdate(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value));

				TestTable.Values("TestTable").Single().Should().Be(value);
			}
		}

		[Test]
		public void ShouldRollbackTransaction()
		{
			using (new TestTable("TestTable"))
			using (var c = buildContainer())
			{
				Assert.Throws<TestException>(() =>
				{
					c.Resolve<Outer>().DoAction(uow =>
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
			using (var c = buildContainer())
			{
				string result = null;
				c.Resolve<Inner1>().Action = uow => { result = uow.CreateSqlQuery("SELECT @@VERSION").List<string>().Single(); };

				c.Resolve<Outer>().ExecuteInners();

				result.Should().Contain("SQL");
			}
		}

		[Test]
		public void ShouldRollbackTransactionForInnerObject()
		{
			using (new TestTable("TestTable"))
			using (var c = buildContainer())
			{
				c.Resolve<Inner1>().Action = s =>
				{
					s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
					throw new TestException();
				};

				Assert.Throws<TestException>(c.Resolve<Outer>().ExecuteInners);

				TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
			}
		}

		[Test]
		public void ShouldSpanTransactionOverAllInnerObjects()
		{
			using (new TestTable("TestTable"))
			using (var c = buildContainer())
			{
				c.Resolve<Inner1>().Action = s => s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (1)").ExecuteUpdate();
				c.Resolve<Inner2>().Action = s =>
				{
					s.CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (2)").ExecuteUpdate();
					throw new TestException();
				};
				Assert.Throws<TestException>(c.Resolve<Outer>().ExecuteInners);

				TestTable.Values("TestTable").Should().Have.Count.EqualTo(0);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForEachThread()
		{
			using (new TestTable("TestTable1"))
			using (new TestTable("TestTable2"))
			using (var c = buildContainer())
			{
				var thread1 = onAnotherThread(() =>
				{
					c.Resolve<Outer>().DoAction(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
				});
				var thread2 = onAnotherThread(() =>
				{
					c.Resolve<Outer>().DoAction(uow =>
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
			using (var c = buildContainer())
			{
				c.Resolve<MutableFakeCurrentHttpContext>().SetContext(new FakeHttpContext());
				c.Resolve<Outer>().DoAction(uow =>
				{
					onAnotherThread(() =>
					{
						var current = c.Resolve<ICurrentReadModelUnitOfWork>();
						current.Current().CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
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
			using (var c = buildContainer())
			{
				var thread1 = onAnotherThread(() =>
				{
					c.Resolve<MutableFakeCurrentHttpContext>().SetContextOnThread(new FakeHttpContext());
					c.Resolve<Outer>().DoAction(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
				});
				var thread2 = onAnotherThread(() =>
				{
					c.Resolve<MutableFakeCurrentHttpContext>().SetContextOnThread(new FakeHttpContext());
					c.Resolve<Outer>().DoAction(uow =>
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
			using (var c = buildContainer())
			{
				var target = c.Resolve<Outer>();

				target.DoAction(uow => { });

				c.Resolve<ICurrentReadModelUnitOfWork>().Current().Should().Be.Null();
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForEachDataSourceOnPrincipal()
		{
			using (new TestTable("TestTable", ConnectionStringHelper.ConnectionStringUsedInTests))
			using (new TestTable("TestTable", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
			using (var c = buildContainer())
			{
				var factory = c.Resolve<IDataSourcesFactory>();
				var dataSource1 = factory.Create("One", ConnectionStringHelper.ConnectionStringUsedInTests, null);
				var dataSource2 = factory.Create("Two", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, null);

				c.Resolve<MutableFakeCurrentTeleoptiPrincipal>().SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource1, null, null), null));
				c.Resolve<Outer>().DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				c.Resolve<MutableFakeCurrentTeleoptiPrincipal>().SetPrincipal(new TeleoptiPrincipal(new TeleoptiIdentity("", dataSource2, null, null), null));
				c.Resolve<Outer>().DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTests).Count().Should().Be(1);
				TestTable.Values("TestTable", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix).Count().Should().Be(1);
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkForDataSourceMatchingRtaConnectionStringIfNoPrincipal()
		{
			// ConfigurationManager.ConnectionStrings["RtaApplication"]
			using (new TestTable("TestTable"))
			using (var c = buildContainer())
			{
				var rtaConnectionString = new SqlConnectionStringBuilder(ConnectionStringHelper.ConnectionStringUsedInTests).ConnectionString;
				rtaConnectionString.Should().Not.Be.EqualTo(ConnectionStringHelper.ConnectionStringUsedInTests);
				c.Resolve<FakeConfigReader>().ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("Wrong", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix),
					new ConnectionStringSettings("RtaApplication", rtaConnectionString)
				};

				var factory = c.Resolve<IDataSourcesFactory>();
				var dataSource1 = factory.Create("Wrong", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, null);
				var dataSource2 = factory.Create("Correct", ConnectionStringHelper.ConnectionStringUsedInTests, null);
				c.Resolve<FakeDataSourcesProvider>().SetAvailableDataSources(new[] { dataSource1, dataSource2 }.Randomize());

				c.Resolve<MutableFakeCurrentTeleoptiPrincipal>().SetPrincipal(null);

				c.Resolve<Outer>().DoUpdate("INSERT INTO TestTable (Value) VALUES (0)");

				TestTable.Values("TestTable").Count().Should().Be(1);
			}
		}

		[Test]
		public void ShouldExecuteCodeAfterSuccessfulCommit()
		{
			using (var c = buildContainer())
			{
				string result = null;
				c.Resolve<Inner1>().ActionWSync = (uow, sync) => sync.OnSuccessfulTransaction(() => result = "success");

				c.Resolve<Outer>().ExecuteInners();

				result.Should().Be("success");
			}
		}


		[Test]
		public void ShouldNotExecuteCodeAfterFailedCommit()
		{
			using (var c = buildContainer())
			{
				var result = "failed";
				c.Resolve<Inner1>().ActionWSync = (uow, sync) =>
				{
					sync.OnSuccessfulTransaction(() => result = "success");
					throw new TestException();
				};

				Assert.Throws<TestException>(c.Resolve<Outer>().ExecuteInners);

				result.Should().Be("failed");
			}
		}

		private static Thread onAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}

		private static IContainer buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());

			builder.RegisterType<Outer>().EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterType<Inner1>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();
			builder.RegisterType<Inner2>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();

			builder.RegisterType<MutableFakeCurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<FakeDataSourcesProvider>().AsSelf().As<IAvailableDataSourcesProvider>().SingleInstance();
			builder.RegisterType<MutableFakeCurrentTeleoptiPrincipal>().AsSelf().As<ICurrentTeleoptiPrincipal>().SingleInstance();
			builder.RegisterType<FakeConfigReader>().As<IConfigReader>().AsSelf().SingleInstance();

			var container = builder.Build();

			var dataSource = container.Resolve<IDataSourcesFactory>().Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null);
			container.Resolve<FakeDataSourcesProvider>().SetAvailableDataSources(new[] { dataSource });
			container.Resolve<FakeConfigReader>().ConnectionStrings = new ConnectionStringSettingsCollection
			{
				new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
			};
			container.Resolve<MutableFakeCurrentTeleoptiPrincipal>().SetPrincipal(null);

			return container;
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