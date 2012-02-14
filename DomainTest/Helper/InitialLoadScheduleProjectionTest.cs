﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class InitialLoadScheduleProjectionTest
	{
		private InitialLoadScheduleProjection target;
		private DateTimePeriod period;

		[SetUp]
		public void Setup()
		{
			period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow);
			target = new InitialLoadScheduleProjection
			         	{
			         		BusinessUnitId = Guid.NewGuid(),
			         		Datasource = "test",
			         		Timestamp = period.StartDateTime
			         	};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.Identity.Should().Not.Be.EqualTo(Guid.Empty);
			target.Timestamp.Should().Be.EqualTo(period.StartDateTime);
			target.BusinessUnitId.Should().Be.EqualTo(target.BusinessUnitId);
			target.Datasource.Should().Be.EqualTo(target.Datasource);
		}
	}
}