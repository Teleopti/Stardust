using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ResourceCalculationReadModelPersisterTest
	{
		public IScheduleForecastSkillReadModelPersister Target { get; set; }

		[Test]
		public void ShouldPersist()
		{
			var skillId = Guid.NewGuid();
			var items = new List<ResourcesDataModel>()
			{
				new ResourcesDataModel()
				{
					Id = skillId,
					Intervals =
						new List<SkillStaffingInterval>()
						{
							new SkillStaffingInterval()
							{
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 16,02,15,0),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16,02,15,0)
							}
						}
				}
			};
			Target.Persist(items, new DateOnly(2016,06,16));

			Target.GetBySkill(skillId, new DateOnly(2016, 06, 16)).Count().Should().Be.EqualTo(1);

		}


		[Test]
		public void ShouldPersistMultipleTimes()
		{
			var skillId = Guid.NewGuid();
			var items = new List<ResourcesDataModel>()
			{
				new ResourcesDataModel()
				{
					Id = skillId,
					Intervals =
						new List<SkillStaffingInterval>()
						{
							new SkillStaffingInterval()
							{
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 16,02,15,0),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16,02,15,0)
							}
						}
				}
			};
			Target.Persist(items, new DateOnly(2016, 06, 16));
			Target.Persist(items, new DateOnly(2016, 06, 16));

		}

		[Test]
		public void ShouldUpdateExistingModel()
		{
			//should we delete the previous skill data and add new one for now
		}

		[Test]
		public void ShouldGetNullIfNotExists()
		{
			
		}
		

		[Test]
		public void ShouldClear()
		{
		
		}
		
	}

	
}