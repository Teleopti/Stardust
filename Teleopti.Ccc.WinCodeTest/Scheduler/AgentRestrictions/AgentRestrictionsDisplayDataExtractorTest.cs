using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayDataExtractorTest
	{
		private MockRepository _mocks;
		private IAgentRestrictionsDisplayDataExtractor _target;
		private IScheduleMatrixPro _matrix;
		private ISchedulingOptions _schedulingOptions;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;

		[SetUp]
		public void Setup()
		{
			
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_target = new AgentRestrictionsDisplayDataExtractor(_workShiftMinMaxCalculator);
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldExtractToDisplayData()
		{
			IAgentDisplayData data = new AgentRestrictionsDisplayRow(_matrix);
			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_workShiftMinMaxCalculator.PossibleMinMaxTimeForPeriod(_matrix, _schedulingOptions)).Return(
					new MinMax<TimeSpan>());
			}

			using(_mocks.Playback())
			{
				_target.ExtractTo(data, _schedulingOptions);
			}

			Assert.AreEqual(new MinMax<TimeSpan>().Minimum, data.MinimumPossibleTime);
			Assert.AreEqual(new MinMax<TimeSpan>().Maximum, data.MaximumPossibleTime);
		}
	}
}