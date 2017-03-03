﻿using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class PersonForOvertimeProvider : IPersonForOvertimeProvider
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public PersonForOvertimeProvider(ICurrentUnitOfWork unitOfWork)
		{
			_currentUnitOfWork = unitOfWork;
		}

		public IList<SuggestedPersonsModel> Persons(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime)
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



	public class FakePersonForOvertimeProvider : IPersonForOvertimeProvider
	{
		private IList<SuggestedPersonsModel> _models;
		public IList<SuggestedPersonsModel> Persons(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime)
		{
			return _models;
		}

		public void Fill(IList<SuggestedPersonsModel> models)
		{
			_models = models;
		}
	}

}