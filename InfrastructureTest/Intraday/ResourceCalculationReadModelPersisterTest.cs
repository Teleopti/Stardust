using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ResourceCalculationReadModelPersisterTest
	{
		public IScheduleForecastSkillReadModelRepository Target { get; set; }
		public ISkillRepository SkillRepository;

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
								EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc),
                                 CalculatedOn = new DateTime(2016, 06, 16, 01, 15, 0,DateTimeKind.Utc)
                            },
							new SkillStaffingInterval()
							{
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 17, 0, 15, 0,DateTimeKind.Utc),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 17, 0, 0, 0,DateTimeKind.Utc),
                                 CalculatedOn = new DateTime(2016, 06, 16, 01, 15, 0,DateTimeKind.Utc)
                            }
						}
				}
			};
			Target.Persist(items);

			Target.GetBySkill(skillId,new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59))
				.Count()
				.Should()
				.Be.EqualTo(1);

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
								StartDateTime = new DateTime(2016, 06, 16,02,15,0),
                                 CalculatedOn = new DateTime(2016, 06, 16, 01, 15, 0,DateTimeKind.Utc)
                            }
						}
				}
			};
			Target.Persist(items);
			Target.Persist(items);

		}

        [Test]
        public void ShouldPersistWithCalculatedDate()
        {
            var skillId = Guid.NewGuid();
            var calculatedOn = new DateTime(2016, 06, 16, 01, 0, 0, DateTimeKind.Utc);
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
                                EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
                                Forecast = 20,
                                StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc),
                                CalculatedOn = calculatedOn
                            }
                        }
                }
            };
            Target.Persist(items);

            var result =  Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59));
            result.First().CalculatedOn.Should().Be.EqualTo(calculatedOn);

        }

        [Test]
        public void ShouldGetLastestDateOfResourceCalculation()
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
                                EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
                                Forecast = 20,
                                StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc),
                                CalculatedOn = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc)
                            },
                            new SkillStaffingInterval()
                            {
                                StaffingLevel = 10,
                                EndDateTime = new DateTime(2016, 06, 16, 0, 15, 0,DateTimeKind.Utc),
                                Forecast = 20,
                                StartDateTime = new DateTime(2016, 06, 16, 0, 0, 0,DateTimeKind.Utc),
                                CalculatedOn = new DateTime(2016, 06, 16, 03, 15, 0,DateTimeKind.Utc)
                            },
                             new SkillStaffingInterval()
                            {
                                StaffingLevel = 10,
                                EndDateTime = new DateTime(2016, 06, 16, 3, 15, 0,DateTimeKind.Utc),
                                Forecast = 20,
                                StartDateTime = new DateTime(2016, 06, 16, 3, 0, 0,DateTimeKind.Utc),
                                 CalculatedOn = new DateTime(2016, 06, 16, 01, 15, 0,DateTimeKind.Utc)
                            }
                        }
                }
            };
            Target.Persist(items);

            var result = Target.GetLastCalculatedTime();
            result.Should().Be.EqualTo(new DateTime(2016, 06, 16, 03, 15, 0, DateTimeKind.Utc));

        }

        [Test]
        public void ShouldPersistForecastWithShrinkage()
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
                                EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
                                Forecast = 20,
                                StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc),
                                CalculatedOn =  new DateTime(2016, 06, 16, 01, 0, 0, DateTimeKind.Utc),
                                ForecastWithShrinkage = 25
                            }
                        }
                }
            };
            Target.Persist(items);

            var result = Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59));
            result.First().ForecastWithShrinkage.Should().Be.EqualTo(25);

        }

    }

	
}