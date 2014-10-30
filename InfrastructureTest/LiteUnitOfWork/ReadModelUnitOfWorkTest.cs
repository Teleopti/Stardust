using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Aop.Core;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;

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

		private static Thread onAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}

		private static IContainer buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<ReadModelUnitOfWorkModule>();

			builder.RegisterType<MutableFakeCurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterType<Outer>().EnableClassInterceptors().InterceptedBy(typeof(AspectInterceptor));
			builder.RegisterType<Inner1>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();
			builder.RegisterType<Inner2>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();

			var container = builder.Build();
			container.Resolve<IReadModelUnitOfWorkConfiguration>()
				.Configure(ConnectionStringHelper.ConnectionStringUsedInTests);
			return container;
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
		public Inner1(ICurrentReadModelUnitOfWork uow)
			: base(uow)
		{
		}
	}

	public class Inner2 : ReadModelUnitOfWorkInnerTester
	{
		public Inner2(ICurrentReadModelUnitOfWork uow)
			: base(uow)
		{
		}
	}

	public class ReadModelUnitOfWorkInnerTester
	{
		private readonly ICurrentReadModelUnitOfWork _uow;

		public ReadModelUnitOfWorkInnerTester(ICurrentReadModelUnitOfWork uow)
		{
			_uow = uow;
		}

		public Action<ILiteUnitOfWork> Action = s => { };

		public void ExecuteAction()
		{
			Action.Invoke(_uow.Current());
		}
	}
}