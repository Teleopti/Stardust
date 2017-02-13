using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class PersonProviderForOvertime : IPersonProviderForOvertime
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public PersonProviderForOvertime(ICurrentUnitOfWork unitOfWork)
		{
			_currentUnitOfWork = unitOfWork;
		}

		public IList<SuggestedPersonsModel> GetPerson(List<Guid> skillIds, DateTime startDateTime, DateTime endDateTime)
		{
			const string sql = "exec StaffingOvertimeSuggestions @SkillIds =:skillIds, "
							   + "@StartDateTime=:startDateTime, @EndDateTime=:endDateTime";
			var result = _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetString("skillIds", string.Join(",", skillIds))
				.SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SuggestedPersonsModel)))
				.List<SuggestedPersonsModel>();
			return result;
		}
	}

	public class SuggestedPersonsModel
	{
		public Guid PersonId { get; set; }
		public DateTime End { get; set; }
		public int TimeToAdd { get; set; }
	}

	public interface IPersonProviderForOvertime
	{
		IList<SuggestedPersonsModel> GetPerson(List<Guid> skillIds, DateTime startDateTime, DateTime endDateTime);
	}
}