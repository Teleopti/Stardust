using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Asm.ViewModelFactory
{
	[TestFixture]
	public class AsmViewModelMappingTest
	{
		private StubFactory scheduleFactory;
		private IProjectionProvider projectionProvider;

		[SetUp]
		public void Setup()
		{
			projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new AsmViewModelMappingProfile(projectionProvider)));
			scheduleFactory = new StubFactory();
		}

		[Test]
		public void ShouldReturnALayerForEachlayerInProjection()
		{
			var scheduleDay1 = MockRepository.GenerateMock<IScheduleDay>();
			var scheduleDay2 = MockRepository.GenerateMock<IScheduleDay>();
			var scheduleDay3 = MockRepository.GenerateMock<IScheduleDay>();

			projectionProvider.Expect(p => p.Projection(scheduleDay1))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("1") }));
			projectionProvider.Expect(p => p.Projection(scheduleDay2))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("2") }));
			projectionProvider.Expect(p => p.Projection(scheduleDay3))
				.Return(scheduleFactory.ProjectionStub(new[] { scheduleFactory.VisualLayerStub("3") }));

			var result = Mapper.Map<IEnumerable<IScheduleDay>, AsmViewModel>(new[] { scheduleDay1, scheduleDay2, scheduleDay3 }).Layers;
			result.Count.Should().Be.EqualTo(3);
			result.Any(l => l.Payload.Equals("1")).Should().Be.True();
		}

	}
}