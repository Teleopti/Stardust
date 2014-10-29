using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Aop.Core;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	[TestFixture]
	public class ReadModelUnitOfWorkTest
	{

		[Test]
		public void ShouldProduceWorkingUnitOfWork()
		{
			using (new TestTable())
			{
				using (var container = BuildContainer())
				{
					var target = container.Resolve<ReadModelUnitOfWorkTester>();
					var value = new Random().Next(-10000, -2).ToString();
					var result = 0;
					target.ExecuteNHibernateSql(s =>
					{
						s.CreateSQLQuery(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value)).ExecuteUpdate();
					});
					target.ExecuteNHibernateSql(s =>
					{
						result = s.CreateSQLQuery("SELECT Value FROM TestTable").List<int>().Single();
					});
					result.Should().Be(value);
				}
			}
		}

		[Test]
		public void ShouldRollbackTransaction()
		{
			using (new TestTable())
			{
				using (var container = BuildContainer())
				{
					var target1 = container.Resolve<ReadModelUnitOfWorkTester>();
					var value = new Random().Next(-10000, -2).ToString();
					Assert.Throws<TestException>(() =>
					{
						target1.ExecuteNHibernateSql(s =>
						{
							s.CreateSQLQuery(string.Format("INSERT INTO TestTable (Value) VALUES ({0})", value)).ExecuteUpdate();
							throw new TestException();
						});
					});

					var target2 = container.Resolve<ReadModelUnitOfWorkTester>();
					IEnumerable<int> result = null;
					target2.ExecuteNHibernateSql(s =>
					{
						result = s.CreateSQLQuery("SELECT Value FROM TestTable").List<int>();
					});
					result.Should().Have.Count.EqualTo(0);
				}
			}
		}

		[Test]
		public void ShouldProduceUnitOfWorkToInnerObjects()
		{
			using (var container = BuildContainer())
			{
				string result = null;
				container.Resolve<ReadModelUnitOfWorkInnerTester1>()
					.Action = s =>
					{
						result = s.CreateSQLQuery("SELECT @@VERSION").List<string>().Single();
					};
				var target = container.Resolve<ReadModelUnitOfWorkTester>();
				target.ExecuteInners();
				result.Should().Contain("SQL");
			}
		}

		[Test]
		public void ShouldRollbackTransactionForInnerObject()
		{
			using (new TestTable())
			{
				using (var container = BuildContainer())
				{
					container.Resolve<ReadModelUnitOfWorkInnerTester1>()
						.Action = s =>
						{
							s.CreateSQLQuery("INSERT INTO TestTable (Value) VALUES (0)").ExecuteUpdate();
							throw new TestException();
						};
					var target1 = container.Resolve<ReadModelUnitOfWorkTester>();
					Assert.Throws<TestException>(target1.ExecuteInners);

					var target2 = container.Resolve<ReadModelUnitOfWorkTester>();
					IEnumerable<int> result = null;
					target2.ExecuteNHibernateSql(s =>
					{
						result = s.CreateSQLQuery("SELECT Value FROM TestTable").List<int>();
					});
					result.Should().Have.Count.EqualTo(0);
				}
			}
		}

		private static IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<AspectsModule>();
			builder.RegisterModule<ReadModelUnitOfWorkModule>();
			builder.RegisterType<ReadModelUnitOfWorkTester>().EnableClassInterceptors();
			builder.RegisterType<ReadModelUnitOfWorkInnerTester1>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkInnerTester2>().AsSelf().As<ReadModelUnitOfWorkInnerTester>().SingleInstance();
			return builder.Build();
		}
	}

	public class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentReadModelUnitOfWork2>()
				.SingleInstance()
				.As<ICurrentReadModelUnitOfWork>();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}

	public class TestException : Exception
	{
	}

	[Intercept(typeof(AspectInterceptor))]
	public class ReadModelUnitOfWorkTester
	{
		private readonly IEnumerable<ReadModelUnitOfWorkInnerTester> _inners;
		private readonly ICurrentReadModelUnitOfWork _uow;

		public ReadModelUnitOfWorkTester(IEnumerable<ReadModelUnitOfWorkInnerTester> inners, ICurrentReadModelUnitOfWork uow)
		{
			_inners = inners;
			_uow = uow;
		}

		[ReadModelUnitOfWork]
		public virtual void ExecuteNHibernateSql(Action<ISession> action)
		{
			action(_uow.Current().Session());
		}

		[ReadModelUnitOfWork]
		public virtual void ExecuteInners()
		{
			_inners.ForEach(i => i.ExecuteAction());
		}
	}

	public class ReadModelUnitOfWorkInnerTester1 : ReadModelUnitOfWorkInnerTester
	{
		public ReadModelUnitOfWorkInnerTester1(ICurrentReadModelUnitOfWork uow)
			: base(uow)
		{
		}
	}

	public class ReadModelUnitOfWorkInnerTester2 : ReadModelUnitOfWorkInnerTester
	{
		public ReadModelUnitOfWorkInnerTester2(ICurrentReadModelUnitOfWork uow)
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

		public Action<ISession> Action = s => { };

		public void ExecuteAction()
		{
			Action.Invoke(_uow.Current().Session());
		}
	}

	public class CurrentReadModelUnitOfWork2 : ICurrentReadModelUnitOfWork
	{
		private ILiteUnitOfWork _uow;

		public void SetCurrentSession(ISession session)
		{
			_uow = new LiteUnitOfWork2(session);
		}

		public ILiteUnitOfWork Current()
		{
			return _uow;
		}
	}

	public class LiteUnitOfWork2 : ILiteUnitOfWork
	{
		private readonly ISession _session;

		public LiteUnitOfWork2(ISession session)
		{
			_session = session;
		}

		public ISession Session()
		{
			return _session;
		}
	}

	public class ReadModelUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public ReadModelUnitOfWorkAttribute()
			: base(typeof(ReadModelUnitOfWorkAspect))
		{
		}
	}

	public class ReadModelUnitOfWorkAspect : IAspect
	{
		private readonly ICurrentReadModelUnitOfWork _uow;
		private ITransaction _transaction;

		public ReadModelUnitOfWorkAspect(ICurrentReadModelUnitOfWork uow)
		{
			_uow = uow;
		}

		public void OnBeforeInvokation()
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, ConnectionStringHelper.ConnectionStringUsedInTests);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			//configuration.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			var sessionFactory = configuration.BuildSessionFactory();
			var session = sessionFactory.OpenSession();

			((CurrentReadModelUnitOfWork2)_uow).SetCurrentSession(session);
			_transaction = session.BeginTransaction();
		}

		public void OnAfterInvokation()
		{
			_transaction.Commit();
		}
	}


	public interface ICurrentReadModelUnitOfWork
	{
		ILiteUnitOfWork Current();
	}

	public interface ILiteUnitOfWork
	{
		ISession Session();
	}



	public class TestTable : IDisposable
	{
		public TestTable()
		{
			applySql("CREATE TABLE TestTable (Value int)");
			//applySql("INSERT INTO TestTable (Value) VALUES (-1)");
		}

		public void Dispose()
		{
			applySql("DROP TABLE TestTable");
		}

		private static void applySql(string Sql)
		{
			using (var connection = new SqlConnection(ConnectionStringHelper.ConnectionStringUsedInTests))
			{
				connection.Open();
				using (var command = new SqlCommand(Sql, connection))
					command.ExecuteNonQuery();
			}
		}
	}

	public class TestTableModel
	{
		public int Value { get; set; }
	}






}
