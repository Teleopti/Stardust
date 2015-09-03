using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool.Controllers
{
	public class ApplicationControllerTest
	{
		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var target = new ApplicationController(null,null,null,null, null);
			var result = target.Index();
			result.ViewName.Should().Be.Empty();
		}

		[Test]
		public void ShouldResetPerformanceCounter()
		{
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var businessUnit = MockRepository.GenerateStub<ICurrentBusinessUnit>();
			var dataSource = MockRepository.GenerateStub<ICurrentDataSource>();
			businessUnit.Stub(x => x.Current().Id).Return(Guid.Empty);
			var target = new ApplicationController(performanceCounter, businessUnit, dataSource, null, null);
			target.ResetPerformanceCounter(1);
			performanceCounter.AssertWasCalled(x=>x.ResetCount());
		}

		[Test]
		public void ShouldPrepareDataForManageAdherenceLoadTest()
		{
			var personGenerator = MockRepository.GenerateStub<IPersonGenerator>();
			var stateGenerator = MockRepository.GenerateStub<IStateGenerator>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			personGenerator.Stub(x => x.Generate(1)).Return(new PersonDataForLoadTest());
			stateGenerator.Stub(x => x.Generate(1)).Return(new[] { "" });
			var target = new ApplicationController(performanceCounter, null, null, personGenerator, stateGenerator);
			target.ManageAdherenceLoadTest(1);
			personGenerator.AssertWasCalled(x => x.Generate(1));
			stateGenerator.AssertWasCalled(x => x.Generate(1));
		}

		[Test]
		public void ShouldReturnJsonConfigForManageAdherenceLoadTest()
		{
			var personGenerator = MockRepository.GenerateStub<IPersonGenerator>();
			var stateGenerator = MockRepository.GenerateStub<IStateGenerator>();
			var performanceCounter = MockRepository.GenerateStub<IPerformanceCounter>();
			var teamId = Guid.NewGuid();
			const string stateCode = "0";
			personGenerator.Stub(x => x.Generate(1))
				.Return(new PersonDataForLoadTest
				{
					Persons = new[] {new PersonWithExternalLogon {ExternalLogOn = "0"}},
					TeamId = teamId
				});
			stateGenerator.Stub(x => x.Generate(1)).Return(new[] {stateCode});
			var target = new ApplicationController(performanceCounter, null, null, personGenerator, stateGenerator);
			var result = (dynamic)target.ManageAdherenceLoadTest(1).Data;

			Assert.That(result.Persons[0].ExternalLogOn, Is.EqualTo("0"));
			Assert.That(result.States[0], Is.EqualTo(stateCode));
			Assert.That(result.TeamId, Is.EqualTo(teamId));
		}


		[Test]
		public void ShouldClearGeneratedPersonData()
		{
			var personGenerator = MockRepository.GenerateStub<IPersonGenerator>();
			var target = new ApplicationController(null, null,null, personGenerator, null);
			target.ClearManageAdherenceLoadTest(1);
			personGenerator.AssertWasCalled(x => x.Clear(1));
		}

	}

}