using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Filters
{
    [TestFixture]
    public class UnitOfWorkActionFilterTest
    {

        [Test]
        public void ShouldCreateUnitOfWorkForEachAction()
        {
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(IUnitOfWorkFactory))).Return(unitOfWorkFactory);

            target.OnActionExecuting(new ActionExecutingContext());

			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork());
        }

        [Test]
        public void ShouldDisposeUnitOfWorkForEachAction()
        {
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(IUnitOfWorkFactory))).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CurrentUnitOfWork()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext());

			unitOfWork.AssertWasCalled(x => x.Dispose());
        }

        [Test]
        public void ShouldHandleNoCurrentUnitOfWork()
        {
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(IUnitOfWorkFactory))).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CurrentUnitOfWork()).Return(null);

            target.OnResultExecuted(new ResultExecutedContext());
        }

		[Test]
		public void ShouldPersistUnitOfWorkForEachAction()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof (IUnitOfWorkFactory))).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CurrentUnitOfWork()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext());

			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

    }
}
