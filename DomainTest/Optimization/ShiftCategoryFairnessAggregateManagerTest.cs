using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ShiftCategoryFairnessAggregateManagerTest
	{
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			
		}


	}

	public class ShiftCategoryFairnessAggregateManager
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;

		public ShiftCategoryFairnessAggregateManager(ISchedulingResultStateHolder resultStateHolder)
		{
			_resultStateHolder = resultStateHolder;
		}
	}
}