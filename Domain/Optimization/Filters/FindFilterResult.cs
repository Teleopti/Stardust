using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.Filters
{
	public class FindFilterResult
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public string FilterType { get; set; }

		public FindFilterResult()
		{
		}

		public FindFilterResult(ITeam team)
		{
			FilterType = FilterModel.TeamFilterType;
			Id = team.Id.Value;
			Name = team.SiteAndTeam;
		}

		public FindFilterResult(ISite site)
		{
			FilterType = FilterModel.SiteFilterType;
			Id = site.Id.Value;
			Name = site.Description.Name;
		}

		public FindFilterResult(IContract contract)
		{
			FilterType = FilterModel.ContractFilterType;
			Id = contract.Id.Value;
			Name = contract.Description.Name;
		}

		public FindFilterResult(ISkill skill)
		{
			FilterType = FilterModel.SkillFilterType;
			Id = skill.Id.Value;
			Name = skill.Name;
		}

		public override bool Equals(object obj)
		{
			var filter = obj as FindFilterResult;
			if (filter == null)
				return false;
			return filter.Id == Id && filter.FilterType == FilterType;
		}

		public override int GetHashCode()
		{
			return $"{FilterType}|{Id}".GetHashCode();
		}
	}
}