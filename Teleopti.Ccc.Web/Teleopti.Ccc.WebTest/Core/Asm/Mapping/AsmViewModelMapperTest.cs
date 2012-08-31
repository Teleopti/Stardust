using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Asm.Mapping
{
	[TestFixture]
	public class AsmViewModelMapperTest
	{
		private StubFactory scheduleFactory;
		private IProjectionProvider projectionProvider;
		private IAsmViewModelMapper target;
		private IUserTimeZone userTimeZone;

		[SetUp]
		public void Setup()
		{
			projectionProvider = MockRepository.GenerateStub<IProjectionProvider>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			scheduleFactory = new StubFactory();
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.Local);
			userTimeZone.Expect(c => c.TimeZone()).Return(timeZone);
			target = new AsmViewModelMapper(projectionProvider, userTimeZone);
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
			var expected = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var result = target.Map(new[]
				               {
				                  scheduleFactory.ScheduleDayStub(new DateTime(2000, 1, 1)),
				                  scheduleFactory.ScheduleDayStub(expected),
										scheduleFactory.ScheduleDayStub(new DateTime(2100, 1, 1))
				               });
			result.StartDate.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldSetStart()
		{
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var expected = TimeZoneHelper.ConvertFromUtc(layerPeriod.StartDateTime, userTimeZone.TimeZone());

			var scheduleDay =scheduleFactory.ScheduleDayStub(layerPeriod.StartDateTime);

			projectionProvider.Expect(p => p.Projection(scheduleDay))
				.Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));

			var result = target.Map(new[] { scheduleDay });

			result.Layers.First().StartJavascriptBaseDate.Should().Be.EqualTo(expected.SubtractJavascriptBaseDate().TotalMilliseconds);
		}

		[Test]
		public void ShouldSetEnd()
		{
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var expected = TimeZoneHelper.ConvertFromUtc(layerPeriod.EndDateTime, userTimeZone.TimeZone());

			var scheduleDay = scheduleFactory.ScheduleDayStub(layerPeriod.StartDateTime);

			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));
			var res = target.Map(new[] {scheduleDay});
			res.Layers.First().EndJavascriptBaseDate.Should().Be.EqualTo(expected.SubtractJavascriptBaseDate().TotalMilliseconds);
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