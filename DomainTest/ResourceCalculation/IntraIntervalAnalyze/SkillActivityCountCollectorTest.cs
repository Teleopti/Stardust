using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	public class SkillActivityCountCollectorTest
	{
		private MockRepository _mock;
		private SkillActivityCountCollector _target;
		private ISkillActivityCounter _counter;
		private List<DateTimePeriod> _periods;
		private DateTimePeriod _interval;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_counter = _mock.StrictMock<ISkillActivityCounter>();
			_target = new SkillActivityCountCollector(_counter);
			_periods = new List<DateTimePeriod>();
			_interval = new DateTimePeriod();
		}

		[Test]
		public void ShouldCollect()
		{
			using (_mock.Record())
			{
				Expect.Call(_counter.Count(_periods, _interval));
			}

			using (_mock.Playback())
			{
				_target.Collect(_periods, _interval);
			}
		}
	}
}
