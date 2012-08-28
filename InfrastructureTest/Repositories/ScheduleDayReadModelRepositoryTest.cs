using System;
using System.Collections.Generic;
using System.Drawing;
using NHibernate.Transform;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class ScheduleDayReadModelRepositoryTest : DatabaseTest
	{
		private ScheduleDayReadModelRepository _target;
 
		[SetUp]
		public void Setup()
		{
			_target = new ScheduleDayReadModelRepository(UnitOfWorkFactory.Current);	
		}

		[Test]
		public void ShouldReturnReadModelsForPerson()
		{
			var dateOnly = new DateOnly(2012, 8, 28);
			var personId = Guid.NewGuid();
			Assert.That(_target.ReadModelsOnPerson(dateOnly, dateOnly.AddDays(5),personId),Is.Not.Null);
		}
	}

	
}