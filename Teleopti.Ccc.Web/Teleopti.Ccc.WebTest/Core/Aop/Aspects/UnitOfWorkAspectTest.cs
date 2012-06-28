using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Aop.Aspects
{
	[TestFixture]
	public class UnitOfWorkAspectTest
	{
		[Test]
		public void ShouldOpenUnitOfWorkBeforeInvokation()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var target = new UnitOfWorkAspect(unitOfWorkFactory);

			target.OnBeforeInvokation();

			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork());
		}

		[Test]
		public void ShouldPersistAndDisposeUnitOfWorkAfterInvocation()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var target = new UnitOfWorkAspect(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);

			target.OnBeforeInvokation();
			target.OnAfterInvokation();

			unitOfWork.AssertWasCalled(x => x.PersistAll());
			unitOfWork.AssertWasCalled(x => x.Dispose());
		}
	}
}
