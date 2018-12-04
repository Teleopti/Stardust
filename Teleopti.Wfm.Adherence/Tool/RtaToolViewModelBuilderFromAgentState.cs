using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Tool
{
	public class RtaToolViewModelBuilderFromAgentState
	{
		private readonly IAgentStateReadModelReader _agentStates;
		private readonly IExternalLogonReader _externalLogons;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly IDataSourceReader _dataSources;

		public RtaToolViewModelBuilderFromAgentState(
			IAgentStateReadModelReader agentStates,
			IExternalLogonReader externalLogons,
			ICommonAgentNameProvider nameDisplaySetting,
			IDataSourceReader dataSources)
		{
			_agentStates = agentStates;
			_externalLogons = externalLogons;
			_nameDisplaySetting = nameDisplaySetting;
			_dataSources = dataSources;
		}

		public IEnumerable<RtaToolViewModel> Build()
		{
			var nameDisplayedAs = _nameDisplaySetting.CommonAgentNameSettings;
			var dataSources = _dataSources.Datasources().ToLookup(x => x.Value, x => x.Key);
			var externalLogOns = _externalLogons.Read().ToArray();
			var agentStates = _agentStates
				.Read(externalLogOns.Select(x => x.PersonId))
				.ToLookup(x => x.PersonId);

			return (
					from externalLogOn in externalLogOns
					let state = agentStates[externalLogOn.PersonId].FirstOrDefault()
					let dataSource = dataSources[externalLogOn.DataSourceId.GetValueOrDefault()].FirstOrDefault()
					select new RtaToolViewModel
					{
						Name = nameDisplayedAs.BuildFor(
							state?.FirstName,
							state?.LastName,
							state?.EmploymentNumber),
						SiteName = state?.SiteName,
						TeamName = state?.TeamName,
						UserCode = externalLogOn.UserCode,
						DataSource = dataSource
					})
				.ToArray().Take(50);
		}

		public IEnumerable<RtaToolViewModel> Build(RtaToolAgentStateFilter filter)
		{
			var nameDisplayedAs = _nameDisplaySetting.CommonAgentNameSettings;
			var dataSources = _dataSources.Datasources().ToLookup(x => x.Value, x => x.Key);
			var externalLogOns = _externalLogons.Read().ToArray();
			var agentStates = _agentStates
				.Read(externalLogOns.Select(x => x.PersonId))
				.ToLookup(x => x.PersonId);

			var vm = (
					from externalLogOn in externalLogOns
					let state = agentStates[externalLogOn.PersonId].FirstOrDefault()
					let dataSource = dataSources[externalLogOn.DataSourceId.GetValueOrDefault()].FirstOrDefault()
					select new 
					{
						Name = nameDisplayedAs.BuildFor(
							state?.FirstName,
							state?.LastName,
							state?.EmploymentNumber),
						SiteName = state?.SiteName,
						SiteId = state?.SiteId,
						TeamName = state?.TeamName,
						TeamId = state?.TeamId,
						UserCode = externalLogOn.UserCode,
						DataSource = dataSource
					})
				.ToArray();
			return vm
				.Where(v => (filter.SiteIds == null || filter.SiteIds.IndexOf(v.SiteId.GetValueOrDefault()) > -1)
							&&
							(filter.TeamIds == null || filter.TeamIds.IndexOf(v.TeamId.GetValueOrDefault()) > -1))
				.Select(v => new RtaToolViewModel
				{
					Name = v.Name,
					SiteName = v.SiteName,
					TeamName = v.TeamName,
					UserCode = v.UserCode,
					DataSource = v.DataSource
				}).Take(50);
		}
	}

}