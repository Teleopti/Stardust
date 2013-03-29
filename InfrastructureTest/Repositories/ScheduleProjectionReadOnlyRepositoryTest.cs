﻿using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture,Category("LongRunning")]
	public class ScheduleProjectionReadOnlyRepositoryTest : DatabaseTest
	{
		private IScheduleProjectionReadOnlyRepository target;
		private IScenario scenario;
		private IPerson person;
		private IAbsence absence;
		private IBudgetGroup budgetGroup;
		private IActivity activity;
		private Guid scenarioId;

		protected override void SetupForRepositoryTest()
		{
			scenario = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(scenario);
			scenarioId = scenario.Id.GetValueOrDefault();

			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);

			absence = AbsenceFactory.CreateAbsence("Vacation");
			PersistAndRemoveFromUnitOfWork(absence);

			budgetGroup = new BudgetGroup{Name = "My Budget",TimeZone = TeleoptiPrincipal.Current.Regional.TimeZone};
			var shrinkage = new CustomShrinkage("test",true);
			shrinkage.AddAbsence(absence);
			budgetGroup.AddCustomShrinkage(shrinkage);
			PersistAndRemoveFromUnitOfWork(budgetGroup);

			activity = ActivityFactory.CreateActivity("test");
			activity.SetId(Guid.NewGuid());

			var site = SiteFactory.CreateSimpleSite();
			PersistAndRemoveFromUnitOfWork(site);

			var team = TeamFactory.CreateSimpleTeam("team test");
			team.Site = site;
			PersistAndRemoveFromUnitOfWork(team);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-1), team, budgetGroup);
			person.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(person);

			target = new ScheduleProjectionReadOnlyRepository(UnitOfWorkFactory.Current);
		}

		[Test]
		public void ShouldIgnoreActivityTime()
		{
			var period =
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var layer = new ProjectionChangedEventLayer
			            	{
			            		ContractTime = TimeSpan.FromHours(8),
			            		WorkTime = TimeSpan.FromHours(8),
			            		DisplayColor = Color.Bisque.ToArgb(),
								Name = "test",
								ShortName = "te",
								StartDateTime = period.StartDateTime,
								EndDateTime = period.EndDateTime,
								PayloadId = activity.Id.GetValueOrDefault()
			            	};
			target.AddProjectedLayer(DateOnly.Today, scenarioId, person.Id.GetValueOrDefault(),layer);
			
			using (NHibernateUnitOfWork unitOfWork = (NHibernateUnitOfWork) UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ISession session = getSession(unitOfWork);
				using (var trans = session.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					setTransaction(unitOfWork,trans);
					target.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(1)),
					                                 budgetGroup, scenario).Count().Should().Be.EqualTo(0);
				}
			}
		}

		private static ISession getSession(IUnitOfWork unitOfWork)
		{
			return (ISession) unitOfWork.GetType().GetField("_session",
			                                                BindingFlags.NonPublic | BindingFlags.Instance |
			                                                BindingFlags.GetField).GetValue(unitOfWork);
		}

		private static void setTransaction(IUnitOfWork unitOfWork,ITransaction transaction)
		{
			unitOfWork.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField).SetValue(unitOfWork, transaction);
		}

		[Test]
		public void ShouldIncludeAbsenceTime()
		{
			var period =
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var layer = new ProjectionChangedEventLayer
			{
				ContractTime = TimeSpan.FromHours(8),
				WorkTime = TimeSpan.FromHours(8),
				DisplayColor = Color.Bisque.ToArgb(),
				Name = "holiday",
				ShortName = "ho",
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				PayloadId = absence.Id.GetValueOrDefault()
			};
			target.AddProjectedLayer(DateOnly.Today, scenarioId, person.Id.GetValueOrDefault(), layer);

			using (NHibernateUnitOfWork unitOfWork = (NHibernateUnitOfWork)UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ISession session = getSession(unitOfWork);
				using (var trans = session.BeginTransaction(IsolationLevel.ReadUncommitted))
				{
					setTransaction(unitOfWork, trans);
					target.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(1)),
													 budgetGroup, scenario).Count().Should().Be.EqualTo(1);
				}
			}
		}

		[Test]
		public void ShouldClearDataForOneDay()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			target.ClearPeriodForPerson(period, scenarioId, person.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldGetIndicationIfReadModelIsLoaded()
		{
			target.IsInitialized().Should().Be.False();
		}
	}
}