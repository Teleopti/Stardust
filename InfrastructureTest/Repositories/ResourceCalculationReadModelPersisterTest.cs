using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ResourceCalculationReadModelPersisterTest
	{
		public IScheduleForecastSkillReadModelRepository Target { get; set; }
		public ISkillRepository SkillRepository;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;

		[Test]
		public void ShouldPersist()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 17, 0, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 17, 0, 0, 0, DateTimeKind.Utc)
					}
				};


			Target.Persist(items, DateTime.Now);

			Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59))
				.Count()
				.Should()
				.Be.EqualTo(1);

		}

		[Test]
		public void ShouldPersistMultipleTimes()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{

					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0)
					}
				};

			Target.Persist(items, DateTime.Now);
			Target.Persist(items, DateTime.Now);

		}

		[Test]
		public void ShouldGetLastestDateOfResourceCalculation()
		{
			Now.Is("2016-06-16 03:15");
			
			var bu1 = BusinessUnitFactory.CreateSimpleBusinessUnit("1");
			
			BusinessUnitRepository.Add(bu1);

			var skillType = SkillTypeFactory.CreateSkillType();
			SkillTypeRepository.Add(skillType);

			var activity = new Activity("activty");
			activity.SetBusinessUnit(bu1);
			ActivityRepository.Add(activity);

			var skill = new Skill("S1", "asdf", Color.AliceBlue, 15, skillType);
			skill.Activity = activity;
			skill.SetBusinessUnit(bu1);
			skill.TimeZone = TimeZoneInfo.Utc;

			SkillRepository.Add(skill);

			CurrentUnitOfWork.Current().PersistAll();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skill.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);
			var result = Target.GetLastCalculatedTime();
			result.First().Value.Should().Be.EqualTo(new DateTime(2016, 06, 16, 03, 15, 0, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldPersistForecastWithShrinkage()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc),
						ForecastWithShrinkage = 25
					}
				};
			Target.Persist(items, DateTime.Now);
			var result = Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59));
			result.First().ForecastWithShrinkage.Should().Be.EqualTo(25);

		}

		[Test]
		public void ShouldPersistReadModelChanges()
		{
			Target.PersistChange(new StaffingIntervalChange()
								 {
									 SkillId = Guid.NewGuid(),
									 StartDateTime = new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc),
									 EndDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
									 StaffingLevel = 1
								 });

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc)));
            changes.Count().Should().Be.EqualTo(1);
			changes.First().StartDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc));
			changes.First().EndDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc));
			changes.First().StaffingLevel.Should().Be.EqualTo(1);

		}

		[Test]
		public void ShouldGetReadmodelChangesWitinGivenPeriod()
		{
			var skillId = Guid.NewGuid();
			Target.PersistChange(new StaffingIntervalChange()
								 {
									 SkillId = skillId,
									 StartDateTime = new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc),
									 EndDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
									 StaffingLevel = 1
								 });

			Target.PersistChange(new StaffingIntervalChange()
								 {
									 SkillId = Guid.NewGuid(),
									 StartDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
									 EndDateTime = new DateTime(2016, 06, 16, 8, 30, 0, DateTimeKind.Utc),
									 StaffingLevel = 1
								 });

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc)));
			changes.Should().Be.Equals(1);
			changes.First().StartDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc));
			changes.First().EndDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc));
			changes.First().StaffingLevel.Should().Be.EqualTo(1);
			changes.First().SkillId.Should().Be.EqualTo(skillId);

		}

		[Test]
		public void ShouldPurgeChangesOlderThanGivenDate()
		{
			var skillWithOldChange = Guid.NewGuid();
			var skillWithNewChange = Guid.NewGuid();
			Now.Is("2016-06-16 08:00");
			Target.PersistChange(new StaffingIntervalChange()
								 {
									 SkillId = skillWithOldChange,
									 StartDateTime = new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc),
									 EndDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
									 StaffingLevel = 1
								 });

			Now.Is("2016-06-16 09:00");
			Target.PersistChange(new StaffingIntervalChange()
								 {
									 SkillId = skillWithNewChange,
									 StartDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
									 EndDateTime = new DateTime(2016, 06, 16, 8, 30, 0, DateTimeKind.Utc),
									 StaffingLevel = 1
								 });

			CurrentUnitOfWork.Current().PersistAll();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillWithOldChange,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, new DateTime(2016, 06, 16, 9, 0, 0, DateTimeKind.Utc));

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 8, 30, 0, DateTimeKind.Utc)));
			changes.Count().Should().Be.EqualTo(1);
			changes.First().StartDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc));
			changes.First().EndDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 8, 30, 0, DateTimeKind.Utc));
			changes.First().SkillId.Should().Be.EqualTo(skillWithNewChange);

		}

		[Test]
		public void ShouldPurgeReadModelDataPerSkill()
		{
			Now.Is("2016-06-16 03:15");
			var skillId1 = Guid.NewGuid();
			var skillId2 = Guid.NewGuid();
			var items = 
						new List<SkillStaffingInterval>()
						{
							new SkillStaffingInterval()
							{
								SkillId = skillId1,
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc)
							}
						
				
			};
			Target.Persist(items, DateTime.Now);

			items =
						new List<SkillStaffingInterval>()
						{
							new SkillStaffingInterval()
							{
								SkillId = skillId2,
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc)
							}


			};

			Target.Persist(items, DateTime.Now);

			Target.GetBySkill(skillId1, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59))
				.Count()
				.Should()
				.Be.EqualTo(1);
			Target.GetBySkill(skillId2, new DateTime(2016, 06, 16, 0, 0, 0), new DateTime(2016, 06, 16, 23, 59, 59))
				.Count()
				.Should()
				.Be.EqualTo(1);

		}

		[Test]
		public void ShouldPurgeReadModelChangeDataPerSkill()
		{
			Now.Is("2016-06-15 03:15");
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skill1,
				StartDateTime = new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});

			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skill2,
				StartDateTime = new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});

			CurrentUnitOfWork.Current().PersistAll();

			var items = 
						new List<SkillStaffingInterval>()
						{
							new SkillStaffingInterval()
							{
								SkillId = skill2,
								StaffingLevel = 10,
								EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0,DateTimeKind.Utc),
								Forecast = 20,
								StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0,DateTimeKind.Utc)
							}
						
				
			};
			Target.Persist(items, DateTime.Now);

			

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 8, 15, 0, DateTimeKind.Utc)));
			changes.Count().Should().Be.EqualTo(1);

		}

		[Test]
		public void LoadAllIntervalsThatAreTouchedByRequestPeriod()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			var result = Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 2, 20, 0, DateTimeKind.Utc),
				new DateTime(2016, 06, 16, 2, 50, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void LoadIntervalsWhereRequestedPeriodIsSameAsInterval()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			var result = Target.GetBySkill(skillId, new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
				new DateTime(2016, 06, 16, 02, 45, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(1);
			result.FirstOrDefault().StartDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc));
			result.FirstOrDefault().EndDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc));
		}

		[Test]
		public void LoadAllChangesThatAreTouchedByRequestPeriod()
		{
			var skillId = Guid.NewGuid();
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 15, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 2, 20, 0, DateTimeKind.Utc),new DateTime(2016, 06, 16, 2, 50, 0, DateTimeKind.Utc)));
			changes.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void LoadChangessWhereRequestedPeriodIsSameAsInterval()
		{
			var skillId = Guid.NewGuid();
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 15, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});

			var changes = Target.GetReadModelChanges(new DateTimePeriod(new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc)));
			changes.Count().Should().Be.EqualTo(1);
			changes.FirstOrDefault().StartDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc));
			changes.FirstOrDefault().EndDateTime.Should().Be.EqualTo(new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc));
		}

        [Test]
        public void LoadIntervallMergedWithChanges()
        {
            Now.Is("2016-06-16 02:15");
            var skillId = Guid.NewGuid();
            var items =
                new List<SkillStaffingInterval>()
                {
                    new SkillStaffingInterval()
                    {
                        SkillId = skillId,
                         StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
                        EndDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
                        StaffingLevel = 10,
                        Forecast = 20
                    },
                       new SkillStaffingInterval()
                    {
                        SkillId = skillId,
                         StartDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
                        EndDateTime = new DateTime(2016, 06, 16, 02, 45, 0, DateTimeKind.Utc),
                        StaffingLevel = 5,
                        Forecast = 10
                    }
                };
            Target.Persist(items, DateTime.Now);

            Target.PersistChange(new StaffingIntervalChange()
            {
                SkillId = skillId,
                StartDateTime = new DateTime(2016, 06, 16, 2, 15, 0, DateTimeKind.Utc),
                EndDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
                StaffingLevel = 1
            });
            Target.PersistChange(new StaffingIntervalChange()
            {
                SkillId = skillId,
                StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
                EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
                StaffingLevel = 1
            });

            var result = Target.ReadMergedStaffingAndChanges(skillId, new DateTimePeriod(new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
                new DateTime(2016, 06, 16, 02, 45, 0, DateTimeKind.Utc)));
            result.Count().Should().Be.EqualTo(2);
            result.First().StaffingLevel.Should().Be.EqualTo(11);
            result.Second().StaffingLevel.Should().Be.EqualTo(6);
        }

		[Test]
		public void NoIntervalsAreReturnedIfMissingDataInReadModelWithNoChanges()
		{
			var skillId = Guid.NewGuid();
		var intervals = Target.ReadMergedStaffingAndChanges(skillId, new DateTimePeriod(new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc)));
			intervals.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void NoIntervalsAreReturnedIfMissingDataInReadModelWithChanges()
		{
			var skillId = Guid.NewGuid();
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 15, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});
			Target.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
				StaffingLevel = 1
			});

			var intervals = Target.ReadMergedStaffingAndChanges(skillId, new DateTimePeriod(new DateTime(2016, 06, 16, 2, 15, 0, DateTimeKind.Utc), new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc)));
			intervals.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetLastExecutedPerBu()
		{
			Now.Is("2016-06-16 03:15");
			var bu1 = BusinessUnitFactory.CreateSimpleBusinessUnit("1");
			var bu2 = BusinessUnitFactory.CreateSimpleBusinessUnit("2");
			BusinessUnitRepository.Add(bu1);
			BusinessUnitRepository.Add(bu2);
			
			var skillType = SkillTypeFactory.CreateSkillType();
			SkillTypeRepository.Add(skillType);

			var activity = new Activity("activty");
			activity.SetBusinessUnit(bu1);
			ActivityRepository.Add(activity);
			
			var skill1 = new Skill("S1", "asdf", Color.AliceBlue, 15, skillType);
			skill1.Activity = activity;
			skill1.SetBusinessUnit(bu1);
			skill1.TimeZone = TimeZoneInfo.Utc;

			activity = new Activity("activty2");
			activity.SetBusinessUnit(bu2);
			ActivityRepository.Add(activity);
			
			var skill2 = new Skill("S2", "asdf", Color.AliceBlue, 15, skillType);
			skill2.Activity = activity;
			skill2.SetBusinessUnit(bu2);
			skill2.TimeZone = TimeZoneInfo.Utc;

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			CurrentUnitOfWork.Current().PersistAll();


			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skill1.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			Now.Is("2016-06-16 03:30");
			items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skill2.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 0, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 0, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			var result = Target.GetLastCalculatedTime();
			result.Count.Should().Be.EqualTo(2);
			result[bu1.Id.GetValueOrDefault()].Should().Be.EqualTo(DateTime.Parse("2016-06-16 03:15"));
			result[bu2.Id.GetValueOrDefault()].Should().Be.EqualTo(DateTime.Parse("2016-06-16 03:30"));
		}

		[Test]
		public void ShouldGetIntervalsForMoreThanOneSkill()
		{
			Now.Is("2016-06-16 03:15");
			var skillId = Guid.NewGuid();
			var skillId2 = Guid.NewGuid();
			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 30, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId2,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 2, 45, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			var result = Target.GetBySkills(new[] { skillId, skillId2 }, new DateTime(2016, 06, 16, 2, 20, 0, DateTimeKind.Utc),
				new DateTime(2016, 06, 16, 2, 50, 0, DateTimeKind.Utc));
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldPurgIfSkillIsDeleted()
		{
			Now.Is("2016-06-16 03:15");
			var bu1 = BusinessUnitFactory.CreateSimpleBusinessUnit("1");
			var bu2 = BusinessUnitFactory.CreateSimpleBusinessUnit("2");
			BusinessUnitRepository.Add(bu1);
			BusinessUnitRepository.Add(bu2);

			var skillType = SkillTypeFactory.CreateSkillType();
			SkillTypeRepository.Add(skillType);

			var activity = new Activity("activty");
			activity.SetBusinessUnit(bu1);
			ActivityRepository.Add(activity);

			var skill1 = new Skill("S1", "asdf", Color.AliceBlue, 15, skillType);
			skill1.Activity = activity;
			skill1.SetBusinessUnit(bu1);
			skill1.TimeZone = TimeZoneInfo.Utc;
			skill1.SetDeleted();

			activity = new Activity("activty2");
			activity.SetBusinessUnit(bu2);
			ActivityRepository.Add(activity);

			var skill2 = new Skill("S2", "asdf", Color.AliceBlue, 15, skillType);
			skill2.Activity = activity;
			skill2.SetBusinessUnit(bu2);
			skill2.TimeZone = TimeZoneInfo.Utc;

			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);

			CurrentUnitOfWork.Current().PersistAll();


			var items =
				new List<SkillStaffingInterval>()
				{
					new SkillStaffingInterval()
					{
						SkillId = skill1.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 0, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skill1.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 02, 30, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 02, 15, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skill2.Id.GetValueOrDefault(),
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 0, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 0, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);
			
			 Target.Purge();
			var result = Target.GetBySkill(skill1.Id.GetValueOrDefault(), new DateTime(2016, 06, 15, 02, 15, 0, DateTimeKind.Utc),
				new DateTime(2016, 06, 17, 02, 15, 0, DateTimeKind.Utc)).ToList();
			result.Count.Should().Be.EqualTo(0);
			result = Target.GetBySkill(skill2.Id.GetValueOrDefault(), new DateTime(2016, 06, 15, 02, 15, 0, DateTimeKind.Utc),
				new DateTime(2016, 06, 17, 02, 15, 0, DateTimeKind.Utc)).ToList();
			result.Count.Should().Be.EqualTo(1);
		}
	
	}
}