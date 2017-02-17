using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool
{
	public class RtaToolViewModelBuilderFromAgentState : IRtaToolViewModelBuilder
	{
		private readonly IAgentStateReadModelLegacyReader _agentStates;
		private readonly IExternalLogonReader _externalLogons;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly IDataSourceReader _dataSources;

		public RtaToolViewModelBuilderFromAgentState(
			IAgentStateReadModelLegacyReader agentStates, 
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
			var dataSources = _dataSources.Datasources();
			var externalLogOns = _externalLogons.Read().ToArray();
			var agentStates = _agentStates.Read(externalLogOns.Select(x => x.PersonId));
			
			return
				(from externalLogOn in externalLogOns
					from dataSource in dataSources
					from state in agentStates
					let name = nameDisplayedAs
						.BuildCommonNameDescription(
							state.FirstName,
							state.LastName,
							state.EmploymentNumber)
					let dataSourceId = externalLogOn.DataSourceId
					where state.PersonId == externalLogOn.PersonId &&
						  dataSource.Value == externalLogOn.DataSourceId.Value
					select new RtaToolViewModel
					{
						Name = name,
						UserCode = externalLogOn.UserCode,
						DataSource = dataSource.Key
					}).ToArray();
		}
	}
}