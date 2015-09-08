﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Deadlock
{
	[TestFixture]
	public class StudentAvailabilityPersistTest : DatabaseTest
	{
		private const int noOfDays = 5;

		[Test]
		public async void ShouldBeAbleToPersistInParallel()
		{
			setup();

			var tasks = new List<Task>();


			for (var day = 1; day <= noOfDays; day++)
			{
				var day1 = day;
				tasks.Add(new Task(() =>
				                   	{
												var date = new DateOnly(2000, 1, day1);
												using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
												{
													var rep = new StudentAvailabilityDayRepository(uow);
													var readIt = rep.Find(date, SetupFixtureForAssembly.loggedOnPerson);
													rep.Remove(readIt.First());
													uow.PersistAll();
												}				                   		
				                   	}
					));
			}

			foreach (var task in tasks)
			{
				task.Start();
			}

			await Task.WhenAll(tasks.ToArray());
		}

		private void setup()
		{
			CleanUpAfterTest();

			for (var day = 1; day <= noOfDays; day++)
			{
				var studentAv = new StudentAvailabilityDay(SetupFixtureForAssembly.loggedOnPerson, new DateOnly(2000, 1, day),
														 new[] { new StudentAvailabilityRestriction() });
				PersistAndRemoveFromUnitOfWork(studentAv);
			}

			UnitOfWork.PersistAll();
		}

	}
}