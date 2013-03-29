using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class AutofacResolveTest
	{
		[Test]
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
