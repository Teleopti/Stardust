﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityDayFormMappingTest
	{
		private TestMappingDependencyResolver _mappingDependencyResolver;
		private ILoggedOnUser _loggedOnUser;

		[SetUp]
		public void Setup()
		{

			_mappingDependencyResolver = new TestMappingDependencyResolver();

			Mapper.Reset();
			Mapper.Initialize(
				c =>
					{
						c.ConstructServicesUsing(_mappingDependencyResolver.Resolve);
						c.AddProfile(new StudentAvailabilityDayFormMappingProfile());
					}
				);

			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var typeConverter = new StudentAvailabilityDayFormMappingProfile.StudentAvailabilityDayFormToStudentAvailabilityDay(_loggedOnUser, Mapper.Engine);
			_mappingDependencyResolver
				.Register(typeConverter)
				.Register(_loggedOnUser)
				;
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapToDestination()
		{
			var destination = new StudentAvailabilityDay(null, DateOnly.Today, new List<IStudentAvailabilityRestriction>{new StudentAvailabilityRestriction()});

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(new StudentAvailabilityDayInput(), destination);

			result.Should().Be.SameInstanceAs(destination);
		}

		[Test]
		public void ShouldMapStartTimeToDestination()
		{
			var destination = new StudentAvailabilityDay(null, DateOnly.Today, new List<IStudentAvailabilityRestriction> { new StudentAvailabilityRestriction() });
			var form = new StudentAvailabilityDayInput { StartTime = new TimeOfDay(TimeSpan.FromHours(7)) };

			Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form, destination);

			destination.RestrictionCollection.Single().StartTimeLimitation.StartTime.Value.Should().Be(form.StartTime.Time);
		}

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(new StudentAvailabilityDayInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapDate()
		{
			var form = new StudentAvailabilityDayInput { Date = DateOnly.Today };

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form);

			result.RestrictionDate.Should().Be(form.Date);
		}

		[Test]
		public void ShouldMapStartTime()
		{
			var form = new StudentAvailabilityDayInput {StartTime = new TimeOfDay(TimeSpan.FromHours(7))};

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form);

			result.RestrictionCollection.Single().StartTimeLimitation.StartTime.Value.Should().Be(form.StartTime.Time);
		}

		[Test]
		public void ShouldMapEndTime()
		{
			var form = new StudentAvailabilityDayInput { EndTime = new TimeOfDay(TimeSpan.FromHours(7)) };

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form);

			result.RestrictionCollection.Single().EndTimeLimitation.EndTime.Value.Should().Be(form.EndTime.Time);
		}

		[Test]
		public void ShouldMapNextDay()
		{
			var form = new StudentAvailabilityDayInput { EndTime = new TimeOfDay(TimeSpan.FromHours(7)), NextDay = true };

			var result = Mapper.Map<StudentAvailabilityDayInput, IStudentAvailabilityDay>(form);

			var nextDayTimeSpan = form.EndTime.Time.Add(TimeSpan.FromDays(1));
			result.RestrictionCollection.Single().EndTimeLimitation.EndTime.Value.Should().Be(nextDayTimeSpan);
		}
	}
}
