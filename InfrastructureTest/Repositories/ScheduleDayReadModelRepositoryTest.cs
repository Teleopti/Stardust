using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class ScheduleDayReadModelRepositoryTest : DatabaseTest
	{
		private ScheduleDayReadModelRepository _target;
 
		[Test]
		public void ShouldReturnReadModelsForPerson()
		{
			_target = new ScheduleDayReadModelRepository(UnitOfWorkFactory.CurrentUnitOfWork());	
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();
			Assert.That(_target.ReadModelsOnPerson(dateOnly, dateOnly.AddDays(5),personId),Is.Not.Null);
		}

		[Test]
		public void ShouldSaveAndLoadReadModel()
		{
			_target = new ScheduleDayReadModelRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			var guid = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			Assert.That(_target.IsInitialized(),Is.False);
			createAndSaveReadModel(guid);
			var ret = _target.ReadModelsOnPerson(dateOnly.AddDays(-1), dateOnly.AddDays(5), guid);
			Assert.That(ret.Count, Is.EqualTo(1));
			Assert.That(_target.IsInitialized(), Is.True);
			_target.ClearPeriodForPerson(new DateOnlyPeriod(dateOnly,dateOnly.AddDays(2)), guid);
			Assert.That(_target.IsInitialized(), Is.False);
		}

		private void createAndSaveReadModel(Guid personId)
		{
			var model = new ScheduleDayReadModel
			{
				Date = new DateTime(2012, 8, 29),
				ContractTimeTicks = TimeSpan.FromHours(8).Ticks,
				WorkTimeTicks = TimeSpan.FromHours(7).Ticks,
				ColorCode = Color.Bisque.ToArgb(),
				StartDateTime = new DateTime(2012, 8, 29, 7, 0, 0),
				EndDateTime = new DateTime(2012, 8, 29, 17, 0, 0),
				Label = "DY",
				PersonId = personId,
				Workday = true
			};
			_target.SaveReadModel(model);
		}
	}

	
}