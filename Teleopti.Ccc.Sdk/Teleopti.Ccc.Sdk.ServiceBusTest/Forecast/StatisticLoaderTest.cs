using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class StatisticLoaderTest
	{
		private MockRepository _mocks;
		private StatisticLoader _target;
		private IStatisticRepository _statisticRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_statisticRepository = _mocks.DynamicMock<IStatisticRepository>();
			_target = new StatisticLoader(_statisticRepository);

		}


	}

	
}