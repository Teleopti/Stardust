using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ResourceCalculationReadModelPersisterTest
	{
		public IScheduleForecastSkillReadModelRepository Target { get; set; }
		public ISkillRepository SkillRepository;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;

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
						EndDateTime = new DateTime(2016, 06, 16, 0, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 0, 0, 0, DateTimeKind.Utc)
					},
					new SkillStaffingInterval()
					{
						SkillId = skillId,
						StaffingLevel = 10,
						EndDateTime = new DateTime(2016, 06, 16, 3, 15, 0, DateTimeKind.Utc),
						Forecast = 20,
						StartDateTime = new DateTime(2016, 06, 16, 3, 0, 0, DateTimeKind.Utc)
					}
				};
			Target.Persist(items, DateTime.Now);

			var result = Target.GetLastCalculatedTime();
			result.Should().Be.EqualTo(new DateTime(2016, 06, 16, 03, 15, 0, DateTimeKind.Utc));
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
		public void ShouldPrugeReadModelDataPerSkill()
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
		public void ShouldPrugeReadModelChangeDataPerSkill()
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

	}


}