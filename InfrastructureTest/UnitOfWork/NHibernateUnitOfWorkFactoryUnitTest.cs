using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class NHibernateUnitOfWorkFactoryUnitTest
	{

		[Test]
		public void ShouldReassociateOnCreateAndOpenUnitOfWork()
		{
			StateHolderReader.Instance.StateReader.Stub(x => x.ApplicationScopeData)
				.Return(MockRepository.GenerateStub<IApplicationData>());
			var session = MockRepository.GenerateMock<ISession>();
			session.Stub(x => x.EnableFilter(null)).IgnoreArguments().Return(MockRepository.GenerateMock<IFilter>()).IgnoreArguments();
			session.Stub(x => x.GetSessionImplementation()).Return(MockRepository.GenerateMock<ISessionImplementor>());
			var sessionFactory = MockRepository.GenerateMock<ISessionFactory>();
			sessionFactory.Stub(x => x.Statistics).Return(MockRepository.GenerateMock<IStatistics>());
			sessionFactory.Stub(x => x.OpenSession((IInterceptor)null)).Return(session).IgnoreArguments();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var target = new NHibernateUnitOfWorkFactoryFake(sessionFactory, () => unitOfWork, MockRepository.GenerateMock<ISessionContextBinder>());
			var root = MockRepository.GenerateMock<IAggregateRoot>();

			target.CreateAndOpenUnitOfWork().Reassociate(root);

			unitOfWork.AssertWasCalled(x => x.Reassociate(root));
		}
	}
}