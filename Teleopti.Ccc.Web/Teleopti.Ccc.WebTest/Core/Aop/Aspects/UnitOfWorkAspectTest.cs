using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Aop.Aspects
{
	[TestFixture]
	public class UnitOfWorkAspectTest
	{
		[Test]
		public void ShouldHaveAttribute()
		{
			var target = new UnitOfWorkAttribute();
			target.Should().Be.AssignableTo<Attribute>();
			target.AspectType.Should().Be(typeof(UnitOfWorkAspect));
		}

		[Test]
		public void ShouldOpenUnitOfWorkBeforeInvokation()
		{
			var uowFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var target = new UnitOfWorkAspect(uowFactoryProvider);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			uowFactoryProvider.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);

			target.OnBeforeInvokation();

			uowFactory.AssertWasCalled(x => x.CreateAndOpenUnitOfWork());
		}

		[Test]
		public void ShouldPersistAndDisposeUnitOfWorkAfterInvocation()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider);
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);

			target.OnBeforeInvokation();
			target.OnAfterInvokation();

			unitOfWork.AssertWasCalled(x => x.PersistAll());
			unitOfWork.AssertWasCalled(x => x.Dispose());
		}
	}
}
