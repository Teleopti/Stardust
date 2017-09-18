using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class PlanningGroupSettingsControllerTest
	{
		[Test]
		public void ShouldCallPersister()
		{
			var model = new PlanningGroupSettingsModel();
			var persister = MockRepository.GenerateMock<IPlanningGroupSettingsModelPersister>();
			var target = new PlanningGroupSettingsController(null, persister);

			target.Persist(model);

			persister.AssertWasCalled(x => x.Persist(model));
		}

		[Test]
		public void ShouldFetchAll()
		{
			var model = new List<PlanningGroupSettingsModel>();
			var fetchModel = MockRepository.GenerateMock<IFetchPlanningGroupSettingsModel>();
			fetchModel.Expect(x => x.FetchAllWithoutPlanningGroup()).Return(model);
			var target = new PlanningGroupSettingsController(fetchModel, null);
			target.FetchAll().Result<IEnumerable<PlanningGroupSettingsModel>>()
				.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldFetchOne()
		{
			var id = Guid.NewGuid();
			var model = new PlanningGroupSettingsModel();

			var fetchModel = MockRepository.GenerateMock<IFetchPlanningGroupSettingsModel>();
			fetchModel.Expect(x => x.Fetch(id)).Return(model);
			var target = new PlanningGroupSettingsController(fetchModel, null);
			target.Fetch(id).Result<PlanningGroupSettingsModel>()
				.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldDelete()
		{
			var id = Guid.NewGuid();
			var persister = MockRepository.GenerateMock<IPlanningGroupSettingsModelPersister>();
			var target = new PlanningGroupSettingsController(null, persister);
			target.Delete(id);
			persister.AssertWasCalled(x => x.Delete(id));
		}
	}
}