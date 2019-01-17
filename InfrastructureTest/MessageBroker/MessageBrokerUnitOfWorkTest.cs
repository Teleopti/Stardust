using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[InfrastructureTest]
	public class MessageBrokerUnitOfWorkTest : IIsolateSystem, IExtendSystem
	{
		public TheService TheService;
		public NestedService1 NestedService1;
		public NestedService2 NestedService2;
		public CurrentHttpContext HttpContext;
		public ICurrentMessageBrokerUnitOfWork UnitOfWork;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TheService>();
			extend.AddService<NestedService1>();
			extend.AddService<NestedService2>();
		}

		public void Isolate(IIsolate isolate)
		{
			var config = new FakeConfigReader();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			isolate.UseTestDouble(config).For<IConfigReader>();
		}

		[Test]
		[TestTable("TestTable")]
		public void ShouldProduceWorkingUnitOfWork()
		{
			var value = new Random().Next(-10000, -2).ToString();

			TheService.DoesUpdate(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value));

			Enumerable.Single(TestTable.Values("TestTable")).Should().Be(value);
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
			NestedService1.Action = uow => { result = Enumerable.Single(uow.CreateSqlQuery("SELECT @@VERSION").List<string>()); };

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
		[Ignore("TEMP")]
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

			Enumerable.Count(TestTable.Values("TestTable1")).Should().Be(1000);
			Enumerable.Count(TestTable.Values("TestTable2")).Should().Be(0);
		}

		[Test]
		[TestTable("TestTable")]
		[Ignore("TEMP")]
		public void ShouldProduceUnitOfWorkForWebRequestSpanning2Threads()
		{
			using(HttpContext.GloballyUse(new FakeHttpContext()))
				TheService.Does(uow =>
				{
					onAnotherThread(() =>
					{
						UnitOfWork.Current().CreateSqlQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
					}).Join();
				});

			Enumerable.Count(TestTable.Values("TestTable")).Should().Be(1);
		}

		[Test]
		[TestTable("TestTable1")]
		[TestTable("TestTable2")]
		[Ignore("TEMP")]
		public void ShouldProduceUnitOfWorkForEachWebRequest()
		{
			var thread1 = onAnotherThread(() =>
			{
				using (HttpContext.OnThisThreadUse(new FakeHttpContext()))
					TheService.Does(uow => 1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable1 (Value) VALUES (0)").ExecuteUpdate()));
			});
			var thread2 = onAnotherThread(() =>
			{
				using (HttpContext.OnThisThreadUse(new FakeHttpContext()))
				{
					TheService.Does(uow =>
					{
						1000.Times(i => uow.CreateSqlQuery("INSERT INTO TestTable2 (Value) VALUES (0)").ExecuteUpdate());
						throw new TestException();
					});
				}
			});
			thread1.Join();
			thread2.Join();

			Enumerable.Count(TestTable.Values("TestTable1")).Should().Be(1000);
			Enumerable.Count(TestTable.Values("TestTable2")).Should().Be(0);
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

		[Test]
		public void ShouldNotAllowNestedUnitOfWork()
		{
			var wasHere = false;

			Assert.Throws<NestedMessageBrokerUnitOfWorkException>(() =>
			{
				TheService.Does(a =>
				{
					TheService.Does(b =>
					{
						wasHere = true;
					});
					wasHere = true;
				});
				wasHere = true;
			});

			wasHere.Should().Be.False();
			UnitOfWork.Current().Should().Be.Null();
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
	}

	[Serializable]
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

		public void BeforeTest(ITest testDetails)
		{
			_table = new TestTable(_name);
		}

		public void AfterTest(ITest testDetails)
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
			: this(name, InfraTestConfigReader.AnalyticsConnectionString)
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
			return Values(tableName, InfraTestConfigReader.AnalyticsConnectionString);
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