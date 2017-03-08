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

			var values = new List<IList<object>>
			{
				new object[]
				{
					typeof(SendLargeBatchesTest).Assembly.GetName().Version.ToString(),
					stopwatch.Elapsed.TotalSeconds,
					"",
					"",
					Environment.MachineName,
					stopwatch.Elapsed
				}
			};

			var appendRequest = service.Spreadsheets.Values
				.Append(new ValueRange { Values = values }, "1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "A1");
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}
	}
}