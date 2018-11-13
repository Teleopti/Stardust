using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
	public abstract class DatabaseTest : INotCompatibleWithIoCTest
	{
		private ISession _session;
		private IPerson _loggedOnPerson;
		private IUnitOfWork _unitOfWork;

		protected ISession Session => _session;
		protected IPerson LoggedOnPerson => _loggedOnPerson;
		protected IUnitOfWork UnitOfWork => _unitOfWork;
		protected ICurrentUnitOfWork CurrUnitOfWork => new ThisUnitOfWork(_unitOfWork);

		[SetUp]
		public void Setup()
		{
			_loggedOnPerson = InfrastructureTestSetup.Before();
			_unitOfWork = InfrastructureTestSetup.DataSource.Application.CreateAndOpenUnitOfWork();
			_session = _unitOfWork.FetchSession();
			SetupForRepositoryTest();
		}

		protected virtual void SetupForRepositoryTest()
		{
		}

		[TearDown]
		public void BaseTeardown()
		{
			_unitOfWork.Dispose();
			_unitOfWork = null;
			InfrastructureTestSetup.After();
		}

		protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
		{
			Session.SaveOrUpdate(obj);
			Session.Flush();
			Session.Evict(obj);
		}

		protected void Logout()
		{
			InfrastructureTestSetup.Logout();
		}
	}
}