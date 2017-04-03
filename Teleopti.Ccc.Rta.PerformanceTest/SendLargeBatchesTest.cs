using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[RtaPerformanceTest]
	[Explicit]
	public class SendLargeBatchesTest
	{
		public StatesSender States;
		public Http Http;

		class serviceaccount_json
		{
			public string client_email { get; set; }
			public string private_key { get; set; }
		}

		[Test]
		public void MeasurePerformance()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			States.SendAllAsLargeBatches();

			stopwatch.Stop();

			var elapsed = stopwatch.Elapsed;

			var json = File.ReadAllText("serviceaccount.json");
			var serviceaccount_json = JsonConvert.DeserializeObject<serviceaccount_json>(json);

			var initializer = new ServiceAccountCredential.Initializer(serviceaccount_json.client_email)
			{
				Scopes = new[]{ SheetsService.Scope.Spreadsheets }
			};
			initializer = initializer.FromPrivateKey(serviceaccount_json.private_key);
			var credential = new ServiceAccountCredential(initializer);

			var service = new SheetsService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "Google Sheets API",
			});

			var headersRequest = service.Spreadsheets.Values
				.Get("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "A1:1");
			headersRequest.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
			var headers = headersRequest.Execute().Values[0];

			var newRow = new object[20];
			newRow[headers.IndexOf("version")] = typeof(SendLargeBatchesTest).Assembly.GetName().Version.ToString();
			newRow[headers.IndexOf("seconds")] = elapsed.TotalSeconds;
			newRow[headers.IndexOf("label")] = "ℹ️";
			newRow[headers.IndexOf("tooltip")] = $@"=INDIRECT(""E""&ROW())&"" - (""&text(INDIRECT(""F""&ROW()), ""hh:MM:ss"")&"")""";
			newRow[headers.IndexOf("agent")] = Environment.MachineName;
			newRow[headers.IndexOf("duration")] = elapsed;

			var appendRequest = service.Spreadsheets.Values
				.Append(new ValueRange
					{
						Values = new List<IList<object>>
						{
							newRow
						}
					},
					"1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE",
					"A1");
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}
	}
}