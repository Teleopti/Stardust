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
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.MessageBrokerUnitOfWork
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddService<TheService>();
			system.AddService<NestedService1>();
			system.AddService<NestedService2>();

			system.UseTestDouble(new MutableFakeCurrentHttpContext()).For<ICurrentHttpContext>();

			system.UseTestDouble(new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("MessageBroker", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix)
				}
			}).For<IConfigReader>();

		}
	}

	[TestFixture]
	[MessageBrokerUnitOfWorkTest]
	public class MessageBrokerUnitOfWorkTest
	{
		public TheService TheService;
		public NestedService1 NestedService1;
		public NestedService2 NestedService2;
		public MutableFakeCurrentHttpContext HttpContext;
		public ICurrentMessageBrokerUnitOfWork UnitOfWork;
		public IDataSourcesFactory DataSourcesFactory;
		public FakeConfigReader ConfigReader;

		[Test]
		[TestTable("TestTable")]
		public void ShouldProduceWorkingUnitOfWork()
		{
			var value = new Random().Next(-10000, -2).ToString();

			TheService.DoesUpdate(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value));

			Enumerable.Single<int>(TestTable.Values("TestTable")).Should().Be(value);
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
			NestedService1.Action = uow => { result = Enumerable.Single<string>(uow.CreateSqlQuery("SELECT @@VERSION").List<string>()); };

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

			Enumerable.Count<int>(TestTable.Values("TestTable1")).Should().Be(1000);
			Enumerable.Count<int>(TestTable.Values("TestTable2")).Should().Be(0);
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

			Enumerable.Count<int>(TestTable.Values("TestTable")).Should().Be(1);
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

			Enumerable.Count<int>(TestTable.Values("TestTable1")).Should().Be(1000);
			Enumerable.Count<int>(TestTable.Values("TestTable2")).Should().Be(0);
		}

		[Test]
		public void ShouldReturnNullWhenNoCurrentUnitOfWork()
		{
			var target = TheService;

			target.Does(uow => { });

			UnitOfWork.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldNotCommitOrDisposeWhenUnitOfWorkNeverUsed()
		{
			var target = TheService;

			Assert.DoesNotThrow(target.DoesNothingWithTheUnitOfWork);
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
		private readonly NestedService1 _nested1;
		private readonly NestedService2 _nested2;
		private readonly ICurrentMessageBrokerUnitOfWork _uow;

		public TheService(NestedService1 nested1, NestedService2 nested2, ICurrentMessageBrokerUnitOfWork uow)
		{
			_nested1 = nested1;
			_nested2 = nested2;
			_uow = uow;
		}

		[MessageBrokerUnitOfWork]
		public virtual void Does(Action<ILiteUnitOfWork> action)
		{
			action(_uow.Current());
		}

		[MessageBrokerUnitOfWork]
		public virtual void DoesNothingWithTheUnitOfWork()
		{
		}

		[MessageBrokerUnitOfWork]
		public virtual void CallNestedServices()
		{
			_nested1.ExecuteAction();
			_nested2.ExecuteAction();
		}
	}

	public class NestedService1 : NestedBase
	{
		public NestedService1(ICurrentMessageBrokerUnitOfWork uow)
			: base(uow)
		{
		}
	}

	public class NestedService2 : NestedBase
	{
		public NestedService2(ICurrentMessageBrokerUnitOfWork uow)
			: base(uow)
		{
		}
	}

	public class NestedBase
	{
		private readonly ICurrentMessageBrokerUnitOfWork _uow;

		public NestedBase(ICurrentMessageBrokerUnitOfWork uow)
		{
			_uow = uow;
		}

		public Action<ILiteUnitOfWork> Action = u => { };

		public void ExecuteAction()
		{
			Action.Invoke(_uow.Current());
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
			: this(name, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix)
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
			return Values(tableName, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
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