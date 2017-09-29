﻿using System;
using System.Web.Http.Results;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TestFixture]
	public class StardustHealthCheckTest
	{
		public StardustController Target;
		public FakeStardustRepository StardustRepository;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new FakeStardustRepository();
			var fakeThisIsAnnoying = new FakeJobStartTimeRepository(new MutableNow());
			Target = new StardustController(StardustRepository, new LegacyFakeEventPublisher(), new FakeTenants(), new StaffingSettingsReader());
		}

		[SetUp]
		public void SetUp()
		{
			StardustRepository.Clear();
		}

		[Test, Ignore("Ignoring, Amanda J can sort this out...")]
		public void ShouldRespond200IfEverythingIsOk()
		{
			StardustRepository.Has(new WorkerNode());
			var result = Target.HealthCheck();
			Assert.IsInstanceOf<OkResult>(result);
		}

		[Test]
		public void ShouldRespond500IfNoNodesRegistered()
		{
			var result = Target.HealthCheck();
			Assert.IsInstanceOf<ExceptionResult>(result);
		}

		[Test]
		public void ShouldRespond500IfHeartbeatsAreOld()
		{
			var node = new WorkerNode {Heartbeat = DateTime.UtcNow.AddHours(-1)};
			StardustRepository.Has(node);
			var result = Target.HealthCheck();
			Assert.IsInstanceOf<ExceptionResult>(result);
		}
	}
}