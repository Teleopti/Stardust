using System;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class QueryFilter : IQueryFilter
	{
		private readonly Action<ISession, object> _enable;

		public static readonly IQueryFilter NoFilter = new QueryFilter("noFilter", (session, p) => { });

		public static readonly IQueryFilter BusinessUnit = new QueryFilter("businessUnitFilter", (session, p) =>
		{
			session.EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", p);
		});

		public static readonly IQueryFilter Deleted = new QueryFilter("deletedFlagFilter", (session, p) =>
		{
			session.EnableFilter("deletedFlagFilter");
		});

		public static readonly IQueryFilter DeletedPeople = new QueryFilter("deletedPeopleFilter", (session, p) =>
		{
			session.EnableFilter("deletedPeopleFilter");
		});

		private QueryFilter(string name, Action<ISession, object> enable)
		{
			_enable = enable;
			InParameter.NotNull(nameof(name), name);
			Name = name;
		}

		public string Name { get; private set; }

		public void Enable(object session, object payload)
		{
			_enable(session as ISession, payload);
		}

		public override bool Equals(object obj)
		{
			var castedObj = obj as QueryFilter;
			return castedObj != null && Name.Equals(castedObj.Name);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}
