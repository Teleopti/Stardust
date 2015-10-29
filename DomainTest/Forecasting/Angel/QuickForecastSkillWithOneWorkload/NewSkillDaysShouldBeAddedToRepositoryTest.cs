using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
{
	public class NewSkillDaysShouldBeAddedToRepositoryTest : QuickForecastTest
	{
		private skillDayRepositoryForTest skillDayRepository;

		protected override ISkillDayRepository SkillDayRepository(ICollection<ISkillDay> existingSkillDays)
		{
			skillDayRepository = new skillDayRepositoryForTest(CurrentSkillDays());
			return skillDayRepository;
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			modifiedSkillDays.Should().Not.Be.Empty();
			modifiedSkillDays.Should().Have.SameValuesAs(skillDayRepository.AddedSkillDays);
		}

		private class skillDayRepositoryForTest : ISkillDayRepository
		{
			private readonly ICollection<ISkillDay> _existingSkillDays;

			public skillDayRepositoryForTest(ICollection<ISkillDay> existingSkillDays)
			{
				_existingSkillDays = existingSkillDays;
				AddedSkillDays = new List<ISkillDay>();
			}

			public List<ISkillDay> AddedSkillDays { get; private set; }

			public void Add(ISkillDay entity)
			{
				AddedSkillDays.Add(entity);
			}

			public void Remove(ISkillDay entity)
			{
				throw new NotImplementedException();
			}

			public ISkillDay Get(Guid id)
			{
				throw new NotImplementedException();
			}

			public IList<ISkillDay> LoadAll()
			{
				throw new NotImplementedException();
			}

			public ISkillDay Load(Guid id)
			{
				throw new NotImplementedException();
			}

			public long CountAllEntities()
			{
				throw new NotImplementedException();
			}

			public void AddRange(IEnumerable<ISkillDay> entityCollection)
			{
				AddedSkillDays.AddRange(entityCollection);
			}

			public IUnitOfWork UnitOfWork { get; private set; }
			public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario)
			{
				return _existingSkillDays;
			}

			public ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario,
				Action<IEnumerable<ISkillDay>> optionalAction)
			{
				throw new NotImplementedException();
			}

			public void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario)
			{
				throw new NotImplementedException();
			}

			public DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario)
			{
				return new DateOnly();
			}

			public ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm)
			{
				throw new NotImplementedException();
			}

			public ICollection<ISkillDay> FindRange(DateOnlyPeriod period, IList<ISkill> skills, IScenario scenario)
			{
				throw new NotImplementedException();
			}

			public IEnumerable<SkillTaskDetailsModel> GetSkillsTasksDetails(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
			{
				throw new NotImplementedException();
			}
		}
	}
}