using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class SchedulePartSignificantPartForDisplayDefinitionsTest
	{
		private MockRepository _mocker;
		private IScheduleDay _mockedPart;
		private IHasDayOffDefinition _hasDayOffDefinition;
		private SchedulePartSignificantPartForDisplayDefinitions _target;

		[SetUp]
		public void Setup()
		{
			_mocker = new MockRepository();
			_mockedPart = _mocker.StrictMock<IScheduleDay>();
			_hasDayOffDefinition = _mocker.StrictMock<IHasDayOffDefinition>();
			_target = new SchedulePartSignificantPartForDisplayDefinitions(_mockedPart, _hasDayOffDefinition);
		}

		[Test]
		public void HasContractDayOffShouldAlwaysReturnFalse()
		{
			bool result = _target.HasContractDayOff();
			Assert.IsFalse(result);
		}
	}
}