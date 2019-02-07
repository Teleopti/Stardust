using System;
using Microsoft.PowerBI.Api.V2;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core.PowerBi
{
	public class FakePowerBiClient: IPowerBIClient
	{
		public void Dispose()
		{
		}

		public FakePowerBiClient(IReports reports)
		{
			Reports = reports;
		}

		public Uri BaseUri { get; set; }
		public JsonSerializerSettings SerializationSettings { get; }
		public JsonSerializerSettings DeserializationSettings { get; }
		public ServiceClientCredentials Credentials { get; }
		public IDatasets Datasets { get; }
		public IImports Imports { get; }
		public IReports Reports { get; }
		public IDashboards Dashboards { get; }
		public ITiles Tiles { get; }
		public IApps Apps { get; }
		public IDataflows Dataflows { get; }
		public IGateways Gateways { get; }
		public IGroups Groups { get; }
		public ICapacities Capacities { get; }
		public IAvailableFeatures AvailableFeatures { get; }
	}
}
