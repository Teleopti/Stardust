using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using IMessageSender = Teleopti.Ccc.Infrastructure.UnitOfWork.IMessageSender;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	/// <summary>
	/// Tests for nhibernate's implementation of IUnitOfWork
	/// </summary>
	[TestFixture]
	[Category("LongRunning")]
	public sealed class NHibernateUnitOfWorkTest
	{
		private IUnitOfWork uow;
		private ISession session;
		private MockRepository mocks;
		private IMessageBrokerComposite messageBroker;
		private ISendPushMessageWhenRootAlteredService pushMessageService;


		/// <summary>
		/// Runs once per test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			session = mocks.StrictMock<ISession>();
			messageBroker = mocks.StrictMock<IMessageBrokerComposite>();
			pushMessageService = mocks.DynamicMock<ISendPushMessageWhenRootAlteredService>();
			uow = new TestUnitOfWork(session, messageBroker, pushMessageService, null);
		}


		[Test]
		public void VerifyRefresh()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			using (mocks.Record())
			{
				Expect.On(session)
					 .Call(session.BeginTransaction())
					 .Return(mocks.StrictMock<ITransaction>());
				session.Refresh(root);
			}
			using (mocks.Playback())
			{
				uow.Refresh(root);
			}
		}

		[Test]
		public void VerifyMerge()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			IAggregateRoot mergedRoot = mocks.StrictMock<IAggregateRoot>();
			using (mocks.Record())
			{
				Expect.On(session)
					  .Call(session.BeginTransaction())
					  .Return(mocks.StrictMock<ITransaction>());
				Expect.Call(root.Id)
					 .Return(Guid.NewGuid());
				Expect.Call(session.Merge(root))
					 .Return(mergedRoot);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(mergedRoot, uow.Merge(root));
			}
		}


		[Test]
		[ExpectedException(typeof(DataSourceException))]
		public void CannotMergeTransientRoot()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			using (mocks.Record())
			{
				Expect.Call(root.Id)
					 .Return(null);
			}
			using (mocks.Playback())
			{
				uow.Merge(root);
			}
		}

		[Test]
		[ExpectedException(typeof(OptimisticLockException))]
		public void VerifyMergeWhenOptimisticLock()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			using (mocks.Record())
			{
				Expect.On(session)
					  .Call(session.BeginTransaction())
					  .Return(mocks.StrictMock<ITransaction>());
				Expect.Call(root.Id).Return(Guid.NewGuid());
				Expect.Call(session.Merge(root))
					 .Throw(new StaleStateException("dummy"));
			}
			using (mocks.Playback())
			{
				uow.Merge(root);
			}
		}

		/// <summary>
		/// Verifies the interceptor can be read.
		/// </summary>
		[Test]
		public void VerifyInterceptorCanBeRead()
		{
			ISessionImplementor sessImpl = mocks.StrictMock<ISessionImplementor>();
			AggregateRootInterceptor interceptor = new AggregateRootInterceptor();
			Expect.On(session)
				  .Call(session.BeginTransaction())
				  .Return(mocks.StrictMock<ITransaction>());
			Expect.On(session)
				 .Call(session.GetSessionImplementation())
				 .Return(sessImpl);
			Expect.Call(sessImpl.Interceptor)
				 .Return(interceptor);
			mocks.ReplayAll();
			TestUnitOfWork testUow = new TestUnitOfWork(session, messageBroker, null, null);
			Assert.AreEqual(testUow.ShowInternalInterceptor, interceptor);
			Assert.AreEqual(testUow.ShowInternalInterceptor, interceptor);
		}

		[Test]
		public void VerifyClearWorks()
		{
			Expect.On(session)
				  .Call(session.BeginTransaction())
				  .Return(mocks.StrictMock<ITransaction>());
			session.Clear();
			mocks.ReplayAll();
			uow.Clear();
			mocks.VerifyAll();
		}

		[Test]
		public void VerifySessionPropertyIsSet()
		{
			Assert.AreSame(session, ((TestUnitOfWork)uow).ShowInternalSession);
		}

		/// <summary>
		/// The nhibernate session must not be null.
		/// </summary>
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SessionMustNotBeNull()
		{
			using (new TestUnitOfWork(null, messageBroker, null, null)) { }
		}

		[Test]
		public void VerifyDisposeDisposesSession()
		{
			var sessionImpl = mocks.DynamicMock<ISessionImplementor>();
			var sessionFactory = mocks.StrictMock<ISessionFactoryImplementor>();
			Expect.Call(session.SessionFactory)
				 .Return(sessionFactory);
			Expect.Call(sessionFactory.CurrentSessionContext)
				 .Return(new ThreadStaticSessionContext(sessionFactory))
				 .Repeat.Any();
			Expect.Call(session.GetSessionImplementation())
				.Return(sessionImpl);
			session.Dispose();
			mocks.ReplayAll();
			uow.Dispose();
			mocks.VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(OptimisticLockException))]
		public void VerifyStaleStateException()
		{
			ITransaction tx = mocks.StrictMock<ITransaction>();
			ISessionImplementor sessImpl = mocks.DynamicMock<ISessionImplementor>();
			AggregateRootInterceptor interceptor = mocks.DynamicMock<AggregateRootInterceptor>();

			using (mocks.Record())
			{
				Expect.On(session)
					 .Call(session.GetSessionImplementation())
					 .Return(sessImpl)
					 .Repeat.Any();
				Expect.Call(sessImpl.Interceptor)
					 .Return(interceptor)
					 .Repeat.Any();
				using (mocks.Ordered())
				{
					Expect.On(session)
						 .Call(session.BeginTransaction())
						 .Return(tx);
					session.Flush();
					LastCall.Throw(new StaleStateException("just for test"));
					Expect.Call(tx.IsActive).Return(true);
					tx.Rollback();
					tx.Dispose();
					Expect.Call(session.IsOpen).Return(true);
					Expect.Call(session.Close()).Return(null);
				}
			}
			using (mocks.Playback())
			{
				uow.PersistAll();
			}
		}

		[Test]
		[ExpectedException(typeof(OptimisticLockException))]
		public void VerifyStaleStateExceptionOnFlush()
		{
			ITransaction tx = mocks.StrictMock<ITransaction>();
			ISessionImplementor sessImpl = mocks.StrictMock<ISessionImplementor>();
			AggregateRootInterceptor interceptor = mocks.StrictMock<AggregateRootInterceptor>();

			using (mocks.Record())
			{
				Expect.On(session)
					 .Call(session.GetSessionImplementation())
					 .Return(sessImpl)
					 .Repeat.Any();
				Expect.Call(sessImpl.Interceptor)
					 .Return(interceptor)
					 .Repeat.Any();
				using (mocks.Ordered())
				{
					Expect.On(session)
						 .Call(session.BeginTransaction())
						 .Return(tx);
					session.Flush();
					LastCall.Throw(new StaleStateException("just for test"));
					Expect.Call(tx.IsActive).Return(true);
					tx.Rollback();
					Expect.Call(session.IsOpen).Return(true);
					Expect.Call(session.Close()).Return(null);
					tx.Dispose();
				}
			}
			using (mocks.Playback())
			{
				uow.Flush();
			}
		}

		[Test]
		public void VerifyPersistAllRollsBackAndClosesSessionIfException()
		{
			ITransaction tx = mocks.StrictMock<ITransaction>();
			ISessionImplementor sessImpl = mocks.StrictMock<ISessionImplementor>();
			AggregateRootInterceptor interceptor = mocks.StrictMock<AggregateRootInterceptor>();

			using (mocks.Record())
			{
				Expect.On(session)
					 .Call(session.GetSessionImplementation())
					 .Return(sessImpl)
					 .Repeat.Any();
				Expect.Call(sessImpl.Interceptor)
					 .Return(interceptor)
					 .Repeat.Any();
				using (mocks.Ordered())
				{
					Expect.On(session)
						 .Call(session.BeginTransaction())
						 .Return(tx);
					session.Flush();
					LastCall.Throw(new ApplicationException("just for test"));
					Expect.Call(tx.IsActive).Return(false);
					Expect.Call(session.IsOpen).Return(false);
				}
			}
			using (mocks.Playback())
			{
				Assert.Throws<DataSourceException>(() =>
								uow.PersistAll());
			}
		}


		[Test]
		public void VerifyPersistAllWorks()
		{
			ITransaction tx = mocks.StrictMock<ITransaction>();
			ISessionImplementor sessImpl = mocks.StrictMock<ISessionImplementor>();
			AggregateRootInterceptor interceptor = new AggregateRootInterceptor();

			using (mocks.Ordered())
			{
				Expect.On(session)
					 .Call(session.GetSessionImplementation())
					 .Return(sessImpl)
					 .Repeat.Any();
				Expect.Call(sessImpl.Interceptor)
					 .Return(interceptor)
					 .Repeat.Any();
				Expect.On(session)
					 .Call(session.BeginTransaction())
					 .Return(tx);
				session.Flush();
				interceptor.Iteration = InterceptorIteration.UpdateRoots;
				session.Flush();
				tx.Commit();
				tx.Dispose();
				Expect.Call(messageBroker.IsAlive).Return(true);
			}

			mocks.ReplayAll();
			uow.PersistAll();
			mocks.VerifyAll();
		}


		/// <summary>
		/// Verifies the is dirty works.
		/// </summary>
		[Test]
		public void VerifyIsDirtyWorks()
		{
			Expect.On(session)
			 .Call(session.BeginTransaction())
			 .Return(mocks.StrictMock<ITransaction>());
			Expect.Call(session.IsDirty()).Return(false);
			Expect.Call(session.IsDirty()).Return(true);
			mocks.ReplayAll();
			Assert.IsFalse(uow.IsDirty());
			Assert.IsTrue(uow.IsDirty());
			mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallDenormalizerOnPersist()
		{
			ITransaction tx = mocks.DynamicMock<ITransaction>();
			ISessionImplementor sessImpl = mocks.StrictMock<ISessionImplementor>();
			IMessageSender messageSender = mocks.StrictMock<IMessageSender>();

			session = mocks.DynamicMock<ISession>();
			messageBroker = mocks.DynamicMock<IMessageBrokerComposite>();
			uow = new TestUnitOfWork(session, messageBroker,pushMessageService,new []{messageSender});

			AggregateRootInterceptor interceptor = new AggregateRootInterceptor();

			using (mocks.Ordered())
			{
				Expect.On(session)
					.Call(session.GetSessionImplementation())
					.Return(sessImpl)
					.Repeat.Any();
				Expect.Call(sessImpl.Interceptor)
					.Return(interceptor)
					.Repeat.Any();
				Expect.On(session)
					.Call(session.BeginTransaction())
					.Return(tx);

				Expect.Call(() => messageSender.Execute(new List<IRootChangeInfo>(interceptor.ModifiedRoots))).IgnoreArguments();
				
				Expect.Call(messageBroker.IsAlive).Return(true);
			}

			mocks.ReplayAll();
			uow.PersistAll();
			mocks.VerifyAll();
		}

		/// <summary>
		/// Verifies that contains works.
		/// </summary>
		[Test]
		public void VerifyContainsWorks()
		{
			IEntity entity = mocks.StrictMock<IEntity>();
			Expect.On(session)
				  .Call(session.BeginTransaction())
				  .Return(mocks.StrictMock<ITransaction>());
			Expect.Call(session.Contains(entity)).Return(false);
			Expect.Call(session.Contains(entity)).Return(true);
			mocks.ReplayAll();
			Assert.IsFalse(uow.Contains(entity));
			Assert.IsTrue(uow.Contains(entity));
			mocks.VerifyAll();
		}

		/// <summary>
		/// Verifies that remove works.
		/// </summary>
		[Test]
		public void VerifyRemoveWorks()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			session.Evict(root);
			Expect.On(session)
				 .Call(session.BeginTransaction())
				 .Return(mocks.StrictMock<ITransaction>());
			mocks.ReplayAll();
			uow.Remove(root);
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyReassociate()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			using (mocks.Record())
			{
				Expect.On(session)
					  .Call(session.BeginTransaction())
					  .Return(mocks.StrictMock<ITransaction>());
				session.Lock(root, LockMode.None);
			}
			using (mocks.Playback())
			{
				uow.Reassociate(root);
			}
		}

		[Test]
		public void VerifyReassociateCollection()
		{
			IAggregateRoot root = mocks.StrictMock<IAggregateRoot>();
			IList<IAggregateRoot> col = new List<IAggregateRoot> { root, root };
			using (mocks.Record())
			{
				Expect.On(session)
					 .Call(session.BeginTransaction())
					 .Return(mocks.StrictMock<ITransaction>());
				session.Lock(root, LockMode.None);
				session.Lock(root, LockMode.None);
			}
			using (mocks.Playback())
			{
				uow.Reassociate(col);
			}
		}


		private class TestUnitOfWork : NHibernateUnitOfWork
		{
			public TestUnitOfWork(ISession mock, IMessageBrokerComposite messageBroker, ISendPushMessageWhenRootAlteredService pushMessageService, IEnumerable<IMessageSender> denormalizers)
				: base(mock, messageBroker, denormalizers, null, pushMessageService, NHibernateUnitOfWorkFactory.UnbindStatic, (s, i) => {}, TransactionIsolationLevel.Default, null)
			{
			}

			public ISession ShowInternalSession
			{
				get { return Session; }
			}

			public IInterceptor ShowInternalInterceptor
			{
				get { return Interceptor; }
			}
		}
	}
}
