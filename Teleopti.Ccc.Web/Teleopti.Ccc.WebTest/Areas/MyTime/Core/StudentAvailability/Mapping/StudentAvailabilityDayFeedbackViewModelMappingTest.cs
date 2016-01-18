using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture, SetCulture("sv-SE")]
	public class StudentAvailabilityDayFeedbackViewModelMappingTest
	{
		private IStudentAvailabilityFeedbackProvider studentAvailabilityFeedbackProvider;

		[SetUp]
		public void Setup()
		{
			studentAvailabilityFeedbackProvider = MockRepository.GenerateMock<IStudentAvailabilityFeedbackProvider>();

			Mapper.Reset();
			Mapper.Initialize(
				x =>
					x.AddProfile(new StudentAvailabilityDayFeedbackViewModelMappingProfile(studentAvailabilityFeedbackProvider,
						new Lazy<IMappingEngine>(() => Mapper.Engine))));
		}

		[Test]
		public void ShouldConfigureCorrectly()
		{
			Mapper.AssertConfigurationIsValid();
		}

		[Test]
		public void ShouldMapDate()
		{
			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);

			result.Date.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);

			var expectedResult = string.Format("{0}-{1}",
				workTimeMinMax.StartTimeLimitation.StartTimeString,
				workTimeMinMax.StartTimeLimitation.EndTimeString);
			result.PossibleStartTimes.Should().Be(expectedResult);
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(19))
			};

			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);

			var expectedResult = string.Format("{0}-{1}", 
				workTimeMinMax.EndTimeLimitation.StartTimeString,
				workTimeMinMax.EndTimeLimitation.EndTimeString);
			result.PossibleEndTimes.Should().Be(expectedResult);
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesLower()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleContractTimeMinutesLower.Should()
				.Be(workTimeMinMax.WorkTimeLimitation.StartTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesUpper()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleContractTimeMinutesUpper.Should()
				.Be(workTimeMinMax.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(null);
			var result = Mapper.Map<DateOnly, StudentAvailabilityDayFeedbackViewModel>(DateOnly.Today);
			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}
	}
}