using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public enum SelectionType
	{
		Skill = 0,
		Org = 1,
		Alarm,
		ExcludeStateGroups
	}

	public struct SelectionInfo
	{
		public string Query { get; set; }
		public Func<ISQLQuery, IQuery> Set { get; set; }
		public SelectionType SelectionType { get; set; }
	}

	public class SelectionInfos
	{
		private readonly List<SelectionInfo> _data = new List<SelectionInfo>();
		public void Add(SelectionInfo info)
		{
			_data.Add(info);
		}

		public bool Any(SelectionType selectionType)
		{
			return _data.Any(x => x.SelectionType == selectionType);
		}

		public string SkillQuery()
		{
			return queryFor(SelectionType.Skill).Single();
		}

		public string OrganizationQuery()
		{
			return "(" + string.Join(" OR ", queryFor(SelectionType.Org)) + ")";
		}
		
		public string ExcludedStateGroupsQuery()
		{
			return "(" + queryFor(SelectionType.ExcludeStateGroups).Single() + ")";
		}

		public string InAlarmQuery()
		{
			return queryFor(SelectionType.Alarm).Single();
		}

		private IEnumerable<string> queryFor(SelectionType selectionType)
		{
			return _data
				.Where(x => x.SelectionType == selectionType)
				.Select(x => x.Query);
		}

		public IEnumerable<Func<ISQLQuery, IQuery>> ParameterFuncs()
		{
			return _data.Select(x => x.Set);
		}
	}
}