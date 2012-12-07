using System;
using System.Drawing;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestsPreparationViewModelMappingProfileTest
	{
		private StubFactory _scheduleFactory;
		private IProjectionProvider _projectionProvider;
		private IUserTimeZone _userTimeZone;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateStub<IProjectionProvider>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = ((TimeZoneInfo)TimeZoneInfoFactory.StockholmTimeZoneInfo());
			_userTimeZone.Expect(c => c.TimeZone()).Return(_timeZone);
			_scheduleFactory = new StubFactory();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeRequestsPreparationViewModelMappingProfile(() => _projectionProvider, () => _userTimeZone)));
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToFalse()
		{
			var domainData = new ShiftTradeRequestsPreparationDomainData();

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.HasWorkflowControlSet.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToTrue()
		{
			var domainData = new ShiftTradeRequestsPreparationDomainData { WorkflowControlSet = new WorkflowControlSet() };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.HasWorkflowControlSet.Should().Be.True();
		}

		[Test]
		public void ShouldMapMyScheduledDayStartTime()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate);
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		_scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
				                                       	}));
			var domainData = new ShiftTradeRequestsPreparationDomainData { MyScheduleDay = scheduleDay };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.MySchedulelayers.First().StartTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(startDate, _timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayEndTime()
		{
			var endDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate);
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		_scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-3), endDate))
				                                       	}));
			var domainData = new ShiftTradeRequestsPreparationDomainData { MyScheduleDay = scheduleDay };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.MySchedulelayers.First().EndTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(endDate, _timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		_scheduleFactory.VisualLayerStub(layerPeriod)
				                                       	}));
			var domainData = new ShiftTradeRequestsPreparationDomainData { MyScheduleDay = scheduleDay };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.MySchedulelayers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
		}

		[Test]
		public void ShouldMapMyScheduledDayPayloadName()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());
			const string activtyName = "Phone";
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		_scheduleFactory.VisualLayerStub(activtyName)
				                                       	}));
			var domainData = new ShiftTradeRequestsPreparationDomainData { MyScheduleDay = scheduleDay };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.MySchedulelayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapMyScheduledDayPayloadColor()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());
			Color activtyColor = Color.Moccasin;
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
				                                       	{
				                                       		_scheduleFactory.VisualLayerStub(activtyColor)
				                                       	}));
			var domainData = new ShiftTradeRequestsPreparationDomainData { MyScheduleDay = scheduleDay };

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.MySchedulelayers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(activtyColor));
		}

	}
}
