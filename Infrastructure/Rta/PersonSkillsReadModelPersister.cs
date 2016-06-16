using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{

	public class PersonSkillsReadModel
	{
		public Guid PersonId { get; set; }
		public Guid[] SkillIds { get; set; }
	}

	public interface IPersonSkillsReadModelPersister
	{
		void Persist(PersonSkillsReadModel personSkillsReadModel);
		void Delete(Guid person);
	}

	public class PersonSkillsReadModelPersister : IPersonSkillsReadModelPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonSkillsReadModelPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(PersonSkillsReadModel model)
		{
			var skills = model.SkillIds
				.Aggregate("", (current, skillId) => current + ($"('{model.PersonId}','{skillId}'),"))
				.TrimEnd(',');
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					$@"
					DELETE FROM [ReadModel].[PersonSkills]
						WHERE PersonId = :personId
					INSERT INTO [ReadModel].[PersonSkills]
						(
							PersonId,
							SkillId
						) VALUES {skills}
					").SetParameter("personId", model.PersonId)
				.ExecuteUpdate();
		}

		public void Delete(Guid person)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"
					DELETE FROM [ReadModel].[PersonSkills]
						WHERE PersonId = :personId
					")
				.SetParameter("personId", person)
				.ExecuteUpdate();
		}
	}
}
