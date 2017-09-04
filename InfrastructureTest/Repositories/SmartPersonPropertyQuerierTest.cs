using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class SmartPersonPropertyQuerierTest : DatabaseTest
	{
		[Test]
		public void ShouldReturnWorkflowControlSetPriorityBasedOnExistingRecords()
		{
			var percentage = new PartTimePercentage("sdf");
			var ctrSched = new ContractSchedule("sdfsdf");
			var person = PersonFactory.CreatePerson("Anders", "Andersson");
			var ctr = new Contract("contract");
			var team = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			var wcs = new WorkflowControlSet("Test");

			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(percentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);
			PersistAndRemoveFromUnitOfWork(wcs);

			person.WorkflowControlSet = wcs;
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(1999, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(person);
			
			var target = new SmartPersonPropertyQuerier(CurrUnitOfWork,new MutableNow(new DateTime(2016,1,1)), CurrentBusinessUnit.Make());
			var suggestions = target.GetWorkflowControlSetSuggestions();
			var suggestion = suggestions.Single();
			suggestion.WorkflowControlSet.Should().Be.EqualTo(wcs.Id.GetValueOrDefault());
			suggestion.Team.Should().Be.EqualTo(team.Id.GetValueOrDefault());
			suggestion.Priority.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnWorkflowControlSetPriorityBasedOnExistingRecordsForMultipleItems()
		{
			var percentage = new PartTimePercentage("sdf");
			var ctrSched = new ContractSchedule("sdfsdf");
			var person1 = PersonFactory.CreatePerson("Anders", "Andersson");
			var person2 = PersonFactory.CreatePerson("Anna", "Andersson");
			var ctr = new Contract("contract");
			var team = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			var wcs = new WorkflowControlSet("Test");

			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(percentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);
			PersistAndRemoveFromUnitOfWork(wcs);

			person1.WorkflowControlSet = wcs;
			person2.WorkflowControlSet = wcs;
			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(1999, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(person1);
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(1999, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(person2);

			var target = new SmartPersonPropertyQuerier(CurrUnitOfWork, new MutableNow(new DateTime(2016, 1, 1)), CurrentBusinessUnit.Make());
			var suggestions = target.GetWorkflowControlSetSuggestions();
			var suggestion = suggestions.Single();
			suggestion.Priority.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnWorkflowControlSetPriorityBasedOnExistingRecordsForDifferenctControlSets()
		{
			var percentage = new PartTimePercentage("sdf");
			var ctrSched = new ContractSchedule("sdfsdf");
			var person1 = PersonFactory.CreatePerson("Anders", "Andersson");
			var person2 = PersonFactory.CreatePerson("Anna", "Andersson");
			var ctr = new Contract("contract");
			var team = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSimpleSite("sdf");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			var wcs1 = new WorkflowControlSet("Test1");
			var wcs2 = new WorkflowControlSet("Test2");

			PersistAndRemoveFromUnitOfWork(ctrSched);
			PersistAndRemoveFromUnitOfWork(percentage);
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(ctr);
			PersistAndRemoveFromUnitOfWork(wcs1);
			PersistAndRemoveFromUnitOfWork(wcs2);

			person1.WorkflowControlSet = wcs1;
			person2.WorkflowControlSet = wcs2;
			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(1999, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(person1);
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(1999, 1, 1), new PersonContract(ctr, percentage, ctrSched), team));
			PersistAndRemoveFromUnitOfWork(person2);

			var target = new SmartPersonPropertyQuerier(CurrUnitOfWork, new MutableNow(new DateTime(2016, 1, 1)), CurrentBusinessUnit.Make());
			var suggestions = target.GetWorkflowControlSetSuggestions();
			suggestions.Should().Have.Count.EqualTo(2);
		}
	}
}