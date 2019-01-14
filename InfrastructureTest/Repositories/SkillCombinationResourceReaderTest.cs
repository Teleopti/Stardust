using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[UnitOfWorkTest]
	public class SkillCombinationResourceReaderTest
	{
		public ISkillCombinationResourceReader Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public INow Now;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		
		
		//mainly tested from SkillCombinationResourceRepositoryTest
		
		[Test]
		public void ShouldReadCombinationResource()
		{
			var skill = persistSkill();
			var now = Now.UtcDateTime();
			var combinationResources = new []
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = now.AddDays(-1),
					EndDateTime = now.AddDays(1),
					Resources = 1,
					SkillIds = new List<Guid>{skill.Id.Value},
					Source="_"
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResources.ToList());
			
			Target.Execute(new DateTimePeriod(now.AddDays(-1), now.AddDays(1))).Any()
				.Should().Be.True();
		}

		private ISkill persistSkill()
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skill = new Skill("skill", "skill", Color.Blue, 15, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			};
			SkillTypeRepository.Add(skillType);
			ActivityRepository.Add(activity);
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();
			return skill;
		}
	}
}