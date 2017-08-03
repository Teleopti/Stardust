using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool
{
	public class RtaToolViewModelBuilderFromAgentState : IRtaToolViewModelBuilder
	{
		private readonly IAgentStateReadModelReader _agentStates;
		private readonly IExternalLogonReader _externalLogons;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly IDataSourceReader _dataSources;
		private static Random _random = new Random();


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

			return
				(
					from vm in
					(
						from externalLogOn in externalLogOns
						let state = agentStates[externalLogOn.PersonId].FirstOrDefault()
						let dataSource = dataSources[externalLogOn.DataSourceId.GetValueOrDefault()].FirstOrDefault()
						select new RtaToolViewModel
						{
							Name = nameDisplayedAs.BuildCommonNameDescription(
								state?.FirstName,
								state?.LastName,
								state?.EmploymentNumber),
							SiteName = state?.SiteName,
							TeamName = state?.TeamName,
							UserCode = externalLogOn.UserCode,
							DataSource = dataSource
						})
					group vm by vm.TeamName
					into g
					select g
				)
				.Select(g => g.OrderBy(v => _random.Next()).Take(15))
				.SelectMany(x => x)
				.ToArray();
		}
	}
}