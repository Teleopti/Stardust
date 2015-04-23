using System;
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
	        var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWorkFactory))).Return(currentUnitOfWorkFactory);
	        currentUnitOfWorkFactory.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
            target.OnActionExecuting(new ActionExecutingContext());

			unitOfWorkFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork());
        }

        [Test]
        public void ShouldDisposeUnitOfWorkForEachAction()
        {
					var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
			currentUnitOfWork.Stub(x => x.Current()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext());

			unitOfWork.AssertWasCalled(x => x.Dispose());
        }

        [Test]
        public void ShouldHandleNoCurrentUnitOfWork()
        {
					var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
			currentUnitOfWork.Stub(x => x.Current()).Return(null);

            target.OnResultExecuted(new ResultExecutedContext());
        }

		[Test]
		public void ShouldPersistUnitOfWorkForEachAction()
		{
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
			currentUnitOfWork.Stub(x => x.Current()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext());

			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

		[Test]
		public void ShouldNotPersistUnitOfWorkWhenNotHandledExceptionOccurs()
		{
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
			currentUnitOfWork.Stub(x => x.Current()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext{Exception = new Exception(),ExceptionHandled = false});

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		[Test]
		public void ShouldPersistUnitOfWorkWhenHandledExceptionOccurs()
		{
			var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
			DependencyResolver.SetResolver(dependencyResolver);
			var target = new UnitOfWorkActionAttribute();

			dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
			currentUnitOfWork.Stub(x => x.Current()).Return(unitOfWork);

			target.OnResultExecuted(new ResultExecutedContext { Exception = new Exception(), ExceptionHandled = true });

			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

	    [Test]
	    public void ShouldDisposeUnitOfWorkEvenWhenException()
	    {
				var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
				var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
				var dependencyResolver = MockRepository.GenerateMock<IDependencyResolver>();
				DependencyResolver.SetResolver(dependencyResolver);
				var target = new UnitOfWorkActionAttribute();

				dependencyResolver.Stub(x => x.GetService(typeof(ICurrentUnitOfWork))).Return(currentUnitOfWork);
				currentUnitOfWork.Stub(x => x.Current()).Return(unitOfWork);

				unitOfWork.Stub(x => x.PersistAll()).Throw(new Exception());

		    try
		    {
					target.OnResultExecuted(new ResultExecutedContext { Exception = new Exception(), ExceptionHandled = true });
		    }
		    catch (Exception)
		    {
		    }

				unitOfWork.AssertWasCalled(x => x.Dispose());
	    }
    }
}
