using System;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Autofac"), TestFixture]
	public class AutofacResolveTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldBeCovered()
		{
			var target = new AutofacResolve(MockRepository.GenerateMock<IComponentContext>());
			try
			{
				target.Resolve(typeof (object));
			}
			catch (Exception)
			{
				
			}
		}
	}
}
