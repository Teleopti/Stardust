using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Asm.ViewModelFactory
{
	[TestFixture]
	public class AsmViewModelMappingTest
	{
		private StubFactory scheduleFactory;
		private IProjectionProvider projectionProvider;
		private IAsmViewModelMapping target;

		[SetUp]
		public void Setup()
		{
			projectionProvider = MockRepository.GenerateStub<IProjectionProvider>();
			scheduleFactory = new StubFactory();
			target = new AsmViewModelMapping(projectionProvider);
		}

		[Test]
		public void ShouldReturnALayerForEachlayerInProjection()
		{
			var scheduleDay1 = scheduleFactory.ScheduleDayStub();
			var scheduleDay2 = scheduleFactory.ScheduleDayStub();
			var scheduleDay3 = scheduleFactory.ScheduleDayStub();

			projectionProvider.Expect(p => p.Projection(scheduleDay1))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("1") }));
			projectionProvider.Expect(p => p.Projection(scheduleDay2))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("2") }));
			projectionProvider.Expect(p => p.Projection(scheduleDay3))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("3") }));

			var result = target.Map(new[] { scheduleDay1, scheduleDay2, scheduleDay3 }).Layers;
			result.Count.Should().Be.EqualTo(3);
			result.Any(l => l.Payload.Equals("1")).Should().Be.True();
		}

		[Test]
		public void ShouldUseEarliestScheduleDayDateAsModelDate()
		{
			var expected = new DateTime(1900, 1, 1);

			var result = target.Map(new[]
				               {
				                  scheduleFactory.ScheduleDayStub(new DateTime(2000, 1, 1)),
				                  scheduleFactory.ScheduleDayStub(expected),
										scheduleFactory.ScheduleDayStub(new DateTime(2100, 1, 1))
				               });
			result.StartDate.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetMinutesFromStartOnLayer()
		{
			var start = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			const int diffInMinutes = 123;

			var scheduleDay =scheduleFactory.ScheduleDayStub(start);

			projectionProvider.Expect(p => p.Projection(scheduleDay))
				.Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(new DateTimePeriod(start.AddMinutes(diffInMinutes),start.AddMinutes(diffInMinutes).AddHours(1)))
				                                       	}));

			var result = target.Map(new[] { scheduleDay });

			result.Layers.First().RelativeStartInMinutes.Should().Be.EqualTo(diffInMinutes);
		}

		[Test]
		public void ShouldSetLayerLength()
		{
			const int length = 1324;
			var startDate = DateTime.UtcNow;
			var layerPeriod = new DateTimePeriod(startDate, startDate.AddMinutes(length));
			var scheduleDay = scheduleFactory.ScheduleDayStub(startDate);
			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));
			var res = target.Map(new[] {scheduleDay});
			res.Layers.First().LengthInMinutes.Should().Be.EqualTo(length);
		}

		[Test]
		public void ShouldSetLayerColor()
		{
			var color = Color.BlanchedAlmond;
			var scheduleDay = scheduleFactory.ScheduleDayStub(new DateTime());
			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(color)
				                                       	}));
			var res = target.Map(new[] { scheduleDay });

			res.Layers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(color));

		}
	}
}