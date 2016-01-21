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
	public class DayOffRulesControllerTest
	{
		[Test]
		public void ShouldCallPersister()
		{
			var model = new DayOffRulesModel();
			var persister = MockRepository.GenerateMock<IDayOffRulesModelPersister>();
			var target = new DayOffRulesController(null, persister);

			target.Persist(model);

			persister.AssertWasCalled(x => x.Persist(model));
		}

		[Test]
		public void ShouldFetchAll()
		{
			var model = new List<DayOffRulesModel>();
			var fetchModel = MockRepository.GenerateMock<IFetchDayOffRulesModel>();
			fetchModel.Expect(x => x.FetchAll()).Return(model);
			var target = new DayOffRulesController(fetchModel, null);
			target.FetchAll().Result<IEnumerable<DayOffRulesModel>>()
				.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldFetchOne()
		{
			var id = Guid.NewGuid();
			var model = new DayOffRulesModel();

			var fetchModel = MockRepository.GenerateMock<IFetchDayOffRulesModel>();
			fetchModel.Expect(x => x.Fetch(id)).Return(model);
			var target = new DayOffRulesController(fetchModel, null);
			target.Fetch(id).Result<DayOffRulesModel>()
				.Should().Be.SameInstanceAs(model);
		}

		[Test]
		public void ShouldDelete()
		{
			var id = Guid.NewGuid();
			var persister = MockRepository.GenerateMock<IDayOffRulesModelPersister>();
			var target = new DayOffRulesController(null, persister);
			target.Delete(id);
			persister.AssertWasCalled(x => x.Delete(id));
		}
	}
}