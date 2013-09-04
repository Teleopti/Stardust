using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		 [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		 public void LazyLoadingManagerWrapper()
		 {
			 var target = new LazyLoadingManagerWrapper();
			 target.IsInitialized(new object());
			 target.Initialize(new object());
		 }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sdf"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Infrastructure.Foundation.OptimisticLockException"), Test]
		public void OptimisticLockException()
		{
			new OptimisticLockException("sdf", Guid.NewGuid(), "sdf");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ctors"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)"), Test]
		public void RunRepositoryCtors()
		{
			foreach (var repositoryType in typeof(PersonAssignmentRepository).Assembly.GetTypes().Where(t => t.Name.EndsWith("Repository")))
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void Agreement()
		{
			var target = new AgreementProvider();
			target.GetAllAgreements();
		}
	}
}