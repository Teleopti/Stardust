using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool
{
	public class RtaToolViewModelBuilderFromRepositories : IRtaToolViewModelBuilder
	{
		private readonly IDataSourceReader _dataSources;
		private readonly IPersonRepository _persons;
		private readonly INow _now;

		public RtaToolViewModelBuilderFromRepositories(IDataSourceReader dataSources,
			IPersonRepository persons,
			INow now)
		{
			_dataSources = dataSources;
			_persons = persons;
			_now = now;
		}

		public IEnumerable<RtaToolViewModel> Build()
		{
			var today = new DateOnly(_now.UtcDateTime());
			var persons = _persons.FindPeopleInOrganization(new DateOnlyPeriod(today, today), false);
			var dataSources = _dataSources.Datasources();
			return (
				from p in persons
				let period = p.Period(today)
				let logon = period.ExternalLogOnCollection.OrderBy(x => x.DataSourceId).FirstOrDefault()
				let external = logon?.AcdLogOnOriginalId
				let d = logon?.DataSourceId
				let datasource = dataSources.FirstOrDefault(x => x.Value == d).Key
				where logon != null
				select new RtaToolViewModel
				{
					Name = p.Name.FirstName + " " + p.Name.LastName,
					UserCode = external,
					DataSource = datasource
				})
				.ToArray();
		}

		public IEnumerable<RtaToolViewModel> Build(RtaToolAgentStateFilter rtaToolAgentStateFilter)
		{
			throw new System.NotImplementedException();
		}
	}
}
