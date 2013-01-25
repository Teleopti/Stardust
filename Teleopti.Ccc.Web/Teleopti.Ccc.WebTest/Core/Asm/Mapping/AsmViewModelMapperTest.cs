﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
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
		private TimeZoneInfo timeZone;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			projectionProvider = MockRepository.GenerateStub<IProjectionProvider>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			scheduleFactory = new StubFactory();
			timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			userTimeZone.Expect(c => c.TimeZone()).Return(timeZone);
			loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person()); 
			target = new AsmViewModelMapper(projectionProvider, userTimeZone, loggedOnUser);
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

			var result = target.Map(new DateTime(2000,1,1), new[] { scheduleDay1, scheduleDay2, scheduleDay3 },0).Layers;
			result.Count().Should().Be.EqualTo(3);
			result.Any(l => l.Payload.Equals("1")).Should().Be.True();
		}

		[Test]
		public void ShouldSetStart()
		{
			var asmZero = new DateTime(1988, 1, 1);
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var expected = TimeZoneHelper.ConvertFromUtc(layerPeriod.StartDateTime, userTimeZone.TimeZone());

			var scheduleDay =scheduleFactory.ScheduleDayStub(layerPeriod.StartDateTime);

			projectionProvider.Expect(p => p.Projection(scheduleDay))
				.Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));

			var result = target.Map(asmZero, new[] { scheduleDay },0);

			result.Layers.First().StartMinutesSinceAsmZero.Should().Be.EqualTo(expected.Subtract(asmZero).TotalMinutes);
		}

		[Test]
		public void ShouldSetEnd()
		{
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var scheduleDay = scheduleFactory.ScheduleDayStub(layerPeriod.StartDateTime);

			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));
			var res = target.Map(new DateTime(2000,1,2), new[] {scheduleDay},0);
			res.Layers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
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
			var res = target.Map(DateTime.Now, new[] { scheduleDay },0);

			res.Layers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(color));
		}

		[Test]
		public void ShouldSetStartTimeText()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = scheduleFactory.ScheduleDayStub(new DateTime());
			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(2)))
				                                       	}));
			var res = target.Map(DateTime.Now, new[] { scheduleDay },0);

			res.Layers.First().StartTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(startDate, timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldSetEndTimeText()
		{
			var endDate = new DateTime(2000, 1, 1, 11, 55, 0, DateTimeKind.Utc);
			var scheduleDay = scheduleFactory.ScheduleDayStub(new DateTime());
			projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-5), endDate))
				                                       	}));
			var res = target.Map(DateTime.Now, new[] { scheduleDay },0);

			res.Layers.First().EndTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(endDate, timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldSetHours()
		{
			//stockholm +1 
			var hoursAsInts = new List<int>();
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));

			var expected = hoursAsInts.ConvertAll(x => x.ToString(loggedOnUser.CurrentUser().PermissionInformation.Culture()));
			var date = new DateTime(2000, 1, 1);
			var scheduleDay = scheduleFactory.ScheduleDayStub(date);
			var res = target.Map(new DateTime(2000,1,1), new[] {scheduleDay},0);
			res.Hours.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldSetHoursWhenWinterBecomesSummer()
		{
			var hoursAsInts = new List<int>();
			hoursAsInts.Add(0);
			hoursAsInts.Add(1);
			//02:00 doesn't exist!
			hoursAsInts.AddRange(Enumerable.Range(3, 21));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.AddRange(Enumerable.Range(0, 24));
			hoursAsInts.Add(0);
			var expected = hoursAsInts.ConvertAll(x => x.ToString(CultureInfo.InvariantCulture));

			var date = new DateTime(2020, 3, 29);

			var scheduleDay = scheduleFactory.ScheduleDayStub();
			var res = target.Map(date, new[] {scheduleDay},0);
			res.Hours.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldSetNumberOfUnreadMessages()
		{
			const int numberOfUnreadMessages = 11;
			var res = target.Map(new DateTime(2020, 3, 29), new[] { scheduleFactory.ScheduleDayStub() }, numberOfUnreadMessages);
			res.UnreadMessageCount.Should().Be.EqualTo(11);
		}
	}
}