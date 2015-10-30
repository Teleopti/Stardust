using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Engine;
using NHibernate.Stat;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	/// <summary>
	/// Tests for UnitOfWorkFactoryTest
	/// </summary>
	[TestFixture, Category("LongRunning")]
	public class NHibernateUnitOfWorkFactoryTest
	{
		private MockRepository mocks;
		private ISessionFactoryImplementor factoryMock;
		private NHibernateUnitOfWorkFactoryFake target;
		private IState stateMock;
		private IStatistics stat;

		/// <summary>
		/// Runs once per test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			factoryMock = mocks.StrictMock<ISessionFactoryImplementor>();
			stateMock = mocks.StrictMock<IState>();
			Settings fakeSettings = new Settings();
			stat = mocks.StrictMock<IStatistics>();
			Expect.On(factoryMock)
				.Call(factoryMock.Settings)
				.Return(fakeSettings)
				.Repeat.Any();
			Expect.On(factoryMock)
				.Call(factoryMock.Statistics)
				.Return(stat)
				.Repeat.Any();
			StateHolderProxyHelper.ClearAndSetStateHolder(mocks, ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person, null, SetupFixtureForAssembly.ApplicationData, SetupFixtureForAssembly.DataSource, stateMock);
		}

		/// <summary>
		/// Can create instance and field data is set.
		/// </summary>
		[Test]
		public void CanCreateAndDataIsSet()
		{
			mocks.ReplayAll();
			target = new NHibernateUnitOfWorkFactoryFake(factoryMock);

			Assert.IsNotNull(target);
			Assert.AreSame(factoryMock, target.SessFactory);
			mocks.VerifyAll();
		}

		/// <summary>
		/// Verifies the can create unit of work when not logged in.
		/// </summary>
		[Test]
		public void VerifyCanCreateUnitOfWorkWhenNotLoggedIn()
		{
			stateMock = mocks.StrictMock<IState>();
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
			using (mocks.Record())
			{
				createSessionMock(factoryMock, Guid.Empty);
				Expect.Call(stateMock.ApplicationScopeData)
					.Return(SetupFixtureForAssembly.ApplicationData)
					.Repeat.Any();
			}
			using (mocks.Playback())
			{
				target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
				target.CreateAndOpenUnitOfWork();
			}
		}

		private ISession createSessionMock(ISessionFactoryImplementor sessionFactory, Guid buId)
		{
			var sessMock = mocks.StrictMock<ISession>();
			var sessImpl = mocks.DynamicMock<ISessionImplementor>();
			IFilter filterBu = mocks.StrictMock<IFilter>();
			IFilter filterDeleted = mocks.StrictMock<IFilter>();
			Expect.Call(sessionFactory.OpenSession(new AggregateRootInterceptor()))
				.IgnoreArguments()
				.Return(sessMock);
			Expect.Call(sessMock.GetSessionImplementation())
				.Return(sessImpl)
				.Repeat.Any();
			sessMock.FlushMode = FlushMode.Never;
			Expect.Call(sessMock.SessionFactory)
				.Return(sessionFactory)
				.Repeat.Any();
			Expect.Call(sessionFactory.CurrentSessionContext)
				.Return(new ThreadStaticSessionContext(sessionFactory))
				.Repeat.Any();
			Expect.On(sessMock)
				.Call(sessMock.EnableFilter("businessUnitFilter"))
				.Return(filterBu);
			Expect.On(filterBu)
				.Call(filterBu.SetParameter("businessUnitParameter", buId))
				.Return(filterBu);
			Expect.On(sessMock)
				.Call(sessMock.EnableFilter("deletedFlagFilter"))
				.Return(filterDeleted);
			Expect.On(sessMock)
				.Call(sessMock.EnableFilter("deletedPeopleFilter"))
				.Return(filterDeleted);
			return sessMock;
		}

		[Test]
		public void CanCreateStatelessUnitOfWork()
		{
			IStatelessSession sessMock = mocks.StrictMock<IStatelessSession>();
			using(mocks.Record())
			{
				Expect.Call(factoryMock.OpenStatelessSession())
					.Return(sessMock);
			}
			using(mocks.Playback())
			{
				target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
				Assert.IsInstanceOf<IStatelessUnitOfWork>(target.CreateAndOpenStatelessUnitOfWork());
			}
		}

		[Test]
		public void VerifyNumberOfUnitOfWorks()
		{
			using(mocks.Record())
			{
				Expect.Call(stat.IsStatisticsEnabled).Return(true);
				Expect.Call(stat.SessionOpenCount).Return(4);
				Expect.Call(stat.SessionCloseCount).Return(2);
			}
			using(mocks.Playback())
			{
				target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
				Assert.AreEqual(2, target.NumberOfLiveUnitOfWorks);
			}
		}

		[Test]
		public void VerifyNumberOfUnitOfWorksReturnNullIfStatisticsNotEnabled()
		{
			using (mocks.Record())
			{
				Expect.Call(stat.IsStatisticsEnabled).Return(false);
			}
			using (mocks.Playback())
			{
				target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
				Assert.IsNull(target.NumberOfLiveUnitOfWorks);
			}
		}

		/// <summary>
		/// Verifies that Dispose() disposes session.
		/// </summary>
		[Test]
		public void VerifyDisposeDisposesSession()
		{
			factoryMock.Dispose();
			mocks.ReplayAll();
			target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
			target.Dispose();
			mocks.VerifyAll();
		}

		[Test]
		public void ShouldReassociateOnCreateAndOpenUnitOfWork()
		{
			var root = MockRepository.GenerateMock<IAggregateRoot>();
			stateMock = mocks.StrictMock<IState>();
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
			using (mocks.Record())
			{
				var session = createSessionMock(factoryMock, Guid.Empty);
				Expect.On(session)
					.Call(session.BeginTransaction())
					.Return(mocks.DynamicMock<ITransaction>())
					.IgnoreArguments();
				Expect.Call(stateMock.ApplicationScopeData)
					.Return(SetupFixtureForAssembly.ApplicationData)
					.Repeat.Any();
				Expect.Call(() => session.Lock(root, LockMode.None));
			}
			using (mocks.Playback())
			{
				target = new NHibernateUnitOfWorkFactoryFake(factoryMock);
				target.CreateAndOpenUnitOfWork().Reassociate(root);
			}
		}
	}
}