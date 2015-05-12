using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autofac;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ConstraintViolationException = Teleopti.Ccc.Infrastructure.Foundation.ConstraintViolationException;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

	[TestFixture]
	[Category("LongRunning")]
	class PersonForScheduleFinderTest : DatabaseTest
	{

		private PersonForScheduleFinder target;

		[Test]
		public void ShouldGetPersonForDayOrTeamOrNameSegment()
		{
			target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

			IBusinessUnit bu = BusinessUnitFactory.CreateSimpleBusinessUnit();
			PersistAndRemoveFromUnitOfWork(bu);

			ISite site = SiteFactory.CreateSimpleSite("d");
			bu.AddSite(site);

			PersistAndRemoveFromUnitOfWork(site);
			ITeam team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.Description = new Description("sdf");
			PersistAndRemoveFromUnitOfWork(team);

			IPerson per1 = PersonFactory.CreatePerson("roger", "kratz");
			IPerson per2 = PersonFactory.CreatePerson("z", "balog");
			IPerson per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));


			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			per1.WorkflowControlSet = workflowControlSet;
			per2.WorkflowControlSet = workflowControlSet;
			per3.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			var result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "roget kra");
			result.ToArray().Length.Should().Be.EqualTo(1);
		}


		private IPersonContract createPersonContract(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}
		
	}
}
