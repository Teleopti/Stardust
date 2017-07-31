using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillCombinationResourceBpoRepositoryTest
	{
		public ISkillCombinationResourceRepository Target;
		public IScenarioRepository ScenarioRepository;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;

		private Guid persistSkill()
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
			var skill = new Skill("skill", "skill", Color.Blue, 15, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			};

			SkillTypeRepository.Add(skillType);
			ActivityRepository.Add(activity);
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();		

			return skill.Id.GetValueOrDefault();
		}

		[Test]
		public void ShouldPersistSingleBpoSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);
			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid> {persistSkill()},
					Source = "TPBrazil"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);
			CurrentUnitOfWork.Current().PersistAll();

			var loadedBpoCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 2016, 12, 21));
			loadedBpoCombinationResources.ToList().Count.Should().Be.EqualTo(1);
			var first = loadedBpoCombinationResources.First();
			first.Resource.Should().Be.EqualTo(1);
			first.StartDateTime.Should().Be.EqualTo(startDate);
			first.EndDateTime.Should().Be.EqualTo(endDate);
			CurrentUnitOfWork.Current().PersistAll();
		}

		[Test]
		public void ShouldPersistDifferentBposSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPParis"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);
			CurrentUnitOfWork.Current().PersistAll();

			var loadedBpoCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 2016, 12, 21)).ToList();
			loadedBpoCombinationResources.Count.Should().Be.EqualTo(2);
			CurrentUnitOfWork.Current().PersistAll();
		}

		[Test]
		public void ShouldNotAddSameBpoTwiceInSourceBpo()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				}
			};
			
			Target.PersistSkillCombinationResourceBpo(combinationResources);
			//CurrentUnitOfWork.Current().PersistAll();

			var bpoList = new Dictionary<Guid, string>();
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				bpoList = Target.LoadSourceBpo(connection);
			}
			bpoList.Count.Should().Be.EqualTo(1);
		}

		[Test, Ignore("ignoring it for now as we will have some way of loading the BPO resources later")]
		public void ShouldCreateSkillCombinationIfMissing()
		{
			var skill1Id = persistSkill();
			var skill2Id = persistSkill();
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{skill1Id,skill2Id},
					Source = "TPBrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillIds = new List<Guid>{skill1Id,skill2Id},
					Source = "TPBrazil"
				}
			};
			Target.PersistSkillCombinationResourceBpo( combinationResources);
			
		}
	}
}