using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SmartPersonPropertyQuerier : ISmartPersonPropertyQuerier
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _businessUnit;
		private const string query = @"SELECT pp.Team,p.WorkflowControlSet, COUNT(*) Priority
			  FROM dbo.Person p 
			  INNER JOIN PersonPeriod pp ON pp.Parent=p.id
			  INNER JOIN WorkflowControlSet w ON p.WorkflowControlSet = w.Id
			  WHERE w.BusinessUnit=:businessUnitId AND
			  p.IsDeleted = 0 AND
			  w.IsDeleted = 0 AND 
			  pp.EndDate >= :currentDate
			  GROUP BY pp.Team,p.WorkflowControlSet
			  ORDER BY Team ASC,COUNT(*) DESC";

		public SmartPersonPropertyQuerier(ICurrentUnitOfWork currentUnitOfWork, INow now, ICurrentBusinessUnit businessUnit)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_businessUnit = businessUnit;
		}

		public IEnumerable<SmartPersonPropertySuggestion> GetWorkflowControlSetSuggestions()
		{
			return session(_currentUnitOfWork.Current()).CreateSQLQuery(query)
				.SetGuid("businessUnitId", _businessUnit.Current().Id.GetValueOrDefault())
				.SetDateOnly("currentDate", _now.ServerDate_DontUse())
				.SetReadOnly(true)
				.SetResultTransformer(Transformers.AliasToBean<SmartPersonPropertySuggestion>())
				.List<SmartPersonPropertySuggestion>();
		}

		private static ISession session(IUnitOfWork uow)
		{
			return ((NHibernateUnitOfWork) uow).Session;
		}
	}

	public struct SmartPersonPropertySuggestion
	{
		public Guid WorkflowControlSet { get; set; }
		public Guid Team { get; set; }
		public int Priority { get; set; }
	}
}