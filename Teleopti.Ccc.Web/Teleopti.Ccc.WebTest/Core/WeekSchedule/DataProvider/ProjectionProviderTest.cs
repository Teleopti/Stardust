using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class ProjectionProviderTest
	{
		private MockRepository mocks;
		private ProjectionProvider target;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			target = new ProjectionProvider();
		}

		[Test]
		public void ShouldCreateProjection()
		{
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var projectionService = mocks.DynamicMock<IProjectionService>();
			var projection = mocks.DynamicMock<IVisualLayerCollection>();

			using (mocks.Record())
			{
				scheduleDay.Expect(x => x.ProjectionService()).Return(projectionService);
				projectionService.Expect(x => x.CreateProjection()).Return(projection);
			}

			using (mocks.Playback())
			{
				var result = target.Projection(scheduleDay);
				result.Should().Be.SameInstanceAs(projection);
			}
		}

	}
}