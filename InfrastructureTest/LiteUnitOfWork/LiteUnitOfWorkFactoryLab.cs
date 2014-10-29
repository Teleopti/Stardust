using System;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Transform;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	[TestFixture]
	public class LiteUnitOfWorkFactoryTest
	{
		[Test]
		public void ShouldProduceWorkingUnitOfWork()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (var uow = factory.MakeUnitOfWork())
				{
					var result = uow.CreateSql("SELECT @@VERSION").List<string>();
					result.Single().Should().Contain("SQL");
				}
			}
		}

	}

	[TestFixture]
	public class LiteUnitOfWorkTest
	{

		[Test]
		public void ShouldExecuteCommonSql()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (var uow = factory.MakeUnitOfWork())
				{
					var result = uow.CreateSql("SELECT @@VERSION").List<string>();
					result.Single().Should().Contain("SQL");
				}
			}
		}

		[Test]
		public void ShouldReadData()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (new TestTable("TestTable"))
				{
					using (var uow = factory.MakeUnitOfWork())
					{
						var result = uow.CreateSql("SELECT Value FROM TestTable")
							.SetResultTransformer(Transformers.AliasToBean(typeof(TestTableModel)))
							.List<TestTableModel>();
						result.Should().Have.Count.GreaterThan(0);
					}
				}
			}
		}

		[Test]
		public void ShouldInsertData()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (new TestTable("TestTable"))
				{
					using (var uow = factory.MakeUnitOfWork())
					{
						uow.CreateSql(@"INSERT INTO TestTable (Value) VALUES (-1290348)").ExecuteUpdate();
						var result = uow.CreateSql("SELECT Value FROM TestTable WHERE Value = -1290348")
							.SetResultTransformer(Transformers.AliasToBean(typeof(TestTableModel)))
							.List<TestTableModel>();
						result.Single().Value.Should().Be(-1290348);
					}
				}
			}
		}

		[Test]
		public void ShouldRollbackTransaction()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (new TestTable("TestTable"))
				{
					using (var uow = factory.MakeUnitOfWork())
					{
						uow.CreateSql(@"INSERT INTO TestTable (Value) VALUES (-1290348)").ExecuteUpdate();
					}
					using (var uow = factory.MakeUnitOfWork())
					{
						var result = uow.CreateSql("SELECT Value FROM TestTable WHERE Value = -1290348")
							.SetResultTransformer(Transformers.AliasToBean(typeof(TestTableModel)))
							.List<TestTableModel>();
						result.Should().Have.Count.EqualTo(0);
					}
				}
			}
		}

		[Test]
		public void ShouldCommitTransaction()
		{
			using (var factory = LiteUnitOfWorkFactoryForTest.Make())
			{
				using (new TestTable("TestTable"))
				{
					using (var uow = factory.MakeUnitOfWork())
					{
						uow.CreateSql(@"INSERT INTO TestTable (Value) VALUES (-1290348)").ExecuteUpdate();
						uow.Commit();
					}
					using (var uow = factory.MakeUnitOfWork())
					{
						var result = uow.CreateSql("SELECT Value FROM TestTable WHERE Value = -1290348")
							.SetResultTransformer(Transformers.AliasToBean(typeof(TestTableModel)))
							.List<TestTableModel>();
						result.Should().Have.Count.EqualTo(1);
					}
				}
			}
		}

	}

	[TestFixture]
	public class CurrentReadModelUnitOfWorkFactoryTest
	{
		[Test]
		public void ShouldBeConfiguredToProduceAWorkingUnitOfWork()
		{
			using (var current = new CurrentReadModelUnitOfWorkFactory())
			{
				current.Configure(ConnectionStringHelper.ConnectionStringUsedInTests);
				using (var uow = current.Current().MakeUnitOfWork())
				{
					var result = uow.CreateSql("SELECT @@VERSION").List<string>();
					result.Single().Should().Contain("SQL");
				}
			}
		}

		[Test]
		public void ShouldReturnSameInstance()
		{
			using (var current = new CurrentReadModelUnitOfWorkFactory())
			{
				current.Configure(ConnectionStringHelper.ConnectionStringUsedInTests);
				current.Current().Should().Be.SameInstanceAs(current.Current());
			}
		}
	}

	[TestFixture]
	public class CurrentReadModelUnitOfWorkTest
	{
		[Test]
		public void ShouldBeConfiguredToProduceAWorkingUnitOfWork()
		{
			using (var factory = new CurrentReadModelUnitOfWorkFactory())
			{
				factory.Configure(ConnectionStringHelper.ConnectionStringUsedInTests);
				var uow = new CurrentReadModelUnitOfWork();
				uow.Configure(factory.Current());
				using (factory.Current().MakeUnitOfWork())
				{
					var result = uow.Current().CreateSql("SELECT @@VERSION").List<string>();
					result.Single().Should().Contain("SQL");
				}
			}
		}
	}

	[TestFixture]
	public class AspectReadModelUnitOfWorkTest
	{

	}

	[TestFixture]
	public class CurrentRtaUnitOfWorkFactoryTest
	{

	}

	[TestFixture]
	public class CurrentRtaUnitOfWorkTest
	{

	}

	[TestFixture]
	public class AspectRtaUnitOfWorkTest
	{

	}

	public class LiteUnitOfWorkFactoryForTest : LiteUnitOfWorkFactory
	{
		public LiteUnitOfWorkFactoryForTest(string connectionString)
			: base(connectionString)
		{
		}

		public static LiteUnitOfWorkFactoryForTest Make()
		{
			return new LiteUnitOfWorkFactoryForTest(
				ConnectionStringHelper.ConnectionStringUsedInTests
				);
		}
	}





	public class CurrentReadModelUnitOfWorkFactory : IDisposable
	{
		private LiteUnitOfWorkFactory _factory;

		public void Configure(string connectionString)
		{
			_factory = new LiteUnitOfWorkFactory(connectionString);
		}

		public LiteUnitOfWorkFactory Current()
		{
			return _factory;
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}


	public class CurrentReadModelUnitOfWork
	{
		private LiteUnitOfWorkFactory _factory;

		public void Configure(LiteUnitOfWorkFactory factory)
		{
			_factory = factory;
		}

		public LiteUnitOfWork Current()
		{
			return new LiteUnitOfWork(_factory.SessionFactory().GetCurrentSession());
		}
	}

	public class LiteUnitOfWorkFactory : IDisposable
	{
		private ISessionFactory _sessionFactory;

		public LiteUnitOfWorkFactory(string connectionString)
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			configuration.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			_sessionFactory = configuration.BuildSessionFactory();
		}

		public LiteUnitOfWork MakeUnitOfWork()
		{
			return new LiteUnitOfWork(_sessionFactory.OpenSession());
		}

		public ISessionFactory SessionFactory()
		{
			return _sessionFactory;
		}

		public void Dispose()
		{
			_sessionFactory.Dispose();
			_sessionFactory = null;
		}
	}

	public class LiteUnitOfWork : IDisposable
	{
		private ISession _session;
		private ITransaction _transaction;

		public LiteUnitOfWork(ISession session)
		{
			CurrentSessionContext.Bind(session);
			_session = session;
			_transaction = _session.BeginTransaction();
		}

		public ISQLQuery CreateSql(string queryString)
		{
			return _session.CreateSQLQuery(queryString);
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Dispose()
		{
			_transaction.Dispose();
			_transaction = null;
			CurrentSessionContext.Unbind(_session.SessionFactory);
			_session.Dispose();
			_session = null;
		}

	}
}
