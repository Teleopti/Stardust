using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
	[TestFixture]
	public class SetupDateTimePeriodToSelectedScheduleTest
	{
		private SchedulePartFactoryForDomain _partFactoryForDomain;
		private MockRepository _mock;
		private ISetupDateTimePeriod _setupDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			_partFactoryForDomain = new SchedulePartFactoryForDomain();
			_mock = new MockRepository();
			_setupDateTimePeriod = _mock.StrictMock<ISetupDateTimePeriod>();
		}

		[Test]
		public void ShouldUsePeriodFromAssignment()
		{
			var schedule = _partFactoryForDomain.CreatePartWithMainShift();
			var expectedtResult = schedule.GetEditorShift().ProjectionService().CreateProjection().Period().Value;
			var target = new SetupDateTimePeriodToSelectedSchedule(schedule, null);
			Assert.AreEqual(expectedtResult, target.Period);					
		}

		[Test]
		public void ShouldUseFallbackIfNoShiftLayers()
		{
			var schedule = _partFactoryForDomain.CreatePart();
			var expectedtResult = new DateTimePeriod(2013, 1, 1, 2013, 1, 1);

			using (_mock.Record())
			{
				Expect.Call(_setupDateTimePeriod.Period).Return(expectedtResult);
			}

			using (_mock.Playback())
			{
				var target = new SetupDateTimePeriodToSelectedSchedule(schedule, _setupDateTimePeriod);
				Assert.AreEqual(expectedtResult, target.Period);
			}		
		}
	}
}
