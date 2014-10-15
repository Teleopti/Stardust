using System;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.LegacyWrappers
{
	public class StatisticHelperFactoryForTest : IStatisticHelperFactory
	{
		private readonly IEnumerable<Action<IStatisticHelper>> _statisticHelperAction;

		public StatisticHelperFactoryForTest(params Action<IStatisticHelper>[] statisticHelperAction)
		{
			_statisticHelperAction = statisticHelperAction;
		}

		public IStatisticHelper Create()
		{
			StatisticHelper = MockRepository.GenerateStub<IStatisticHelper>();
			_statisticHelperAction.ForEach(x => x(StatisticHelper));
			return StatisticHelper;
		}

		public IStatisticHelper StatisticHelper { get; private set; }
	}
}