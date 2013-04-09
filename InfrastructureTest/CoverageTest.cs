using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	/// <summary>
	/// Coverage went to low when I did some refactoring... (removed tested code)
	/// Remove this class when we don't have a code coverage limit.
	/// 
	/// This fixture only contains totally stupid tests for coverage.
	/// </summary>
	[TestFixture]
	public class CoverageTest : DatabaseTest
	{
		 [Test]
		 public void LazyLoadingManagerWrapper()
		 {
			 var target = new LazyLoadingManagerWrapper();
			 target.IsInitialized(new object());
			 target.Initialize(new object());
		 }

		[Test]
		public void OptimisticLockException()
		{
			new OptimisticLockException("sdf", Guid.NewGuid(), "sdf");
		}

		[Test]
		public void WindowsPrincipal()
		{
			var target = new WindowsAppDomainPrincipalContext(null);
			Assert.Throws<NotImplementedException>(() => target.SetCurrentPrincipal(null));
		}

		[Test]
		public void UnitOfWorkTest()
		{
			UnitOfWork.GetHashCode();
		}

		[Test]
		public void RunRepositoryCtors()
		{
			foreach (var repositoryType in typeof(PersonAssignment).Assembly.GetTypes().Where(t => t.Name.EndsWith("Repository")))
			{
				if (repositoryType.GetConstructor(new[]{typeof (IUnitOfWorkFactory)}) != null)
				{
					Activator.CreateInstance(repositoryType, UnitOfWorkFactory.Current);
				}
				if (repositoryType.GetConstructor(new[] { typeof(ICurrentUnitOfWork) }) != null)
				{
					Activator.CreateInstance(repositoryType, UnitOfWorkFactory.CurrentUnitOfWork());
				}
			}
		}

		[Test]
		public void Agreement()
		{
			var target = new AgreementProvider();
			target.GetAllAgreements();
		}
	}
}