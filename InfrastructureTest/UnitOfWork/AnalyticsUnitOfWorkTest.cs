using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[InfrastructureTest]
	public class AnalyticsUnitOfWorkTest : ISetup
	{
		public TheService TheService;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		[Test]
		public void ShouldNotAllowNestedUnitOfWork()
		{
			var wasHere = false;

			Assert.Throws<NestedAnalyticsUnitOfWorkException>(() =>
			{
				TheService.Does(a =>
				{
					TheService.Does(b =>
					{
						wasHere = true;
					});
					wasHere = true;
				});
				wasHere = true;
			});

			wasHere.Should().Be.False();
			UnitOfWork.Current().Should().Be.Null();
		}
		
	}

	public class TheService
	{
		private readonly ICurrentAnalyticsUnitOfWork _uow;

		public TheService(ICurrentAnalyticsUnitOfWork uow)
		{
			_uow = uow;
		}

		[AnalyticsUnitOfWork]
		public virtual void Does(Action<IUnitOfWork> action)
		{
			action(_uow.Current());
		}
	}

}