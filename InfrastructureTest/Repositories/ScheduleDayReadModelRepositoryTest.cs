using System;
using NUnit.Framework;
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
			_target = new ScheduleDayReadModelRepository(UnitOfWorkFactory.Current);	
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();
			Assert.That(_target.ReadModelsOnPerson(dateOnly, dateOnly.AddDays(5),personId),Is.Not.Null);
		}
	}

	
}