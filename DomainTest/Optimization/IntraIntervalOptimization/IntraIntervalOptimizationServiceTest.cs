using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalOptimizationServiceTest
	{
		private IntraIntervalOptimizationService _target;

		[SetUp]
		public void SetUp()
		{
			_target = new IntraIntervalOptimizationService();	
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void ShouldThrowNotImplemented()
		{
			_target.Execute();	
		}
	}
}
