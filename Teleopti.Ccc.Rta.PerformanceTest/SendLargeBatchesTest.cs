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
			var startTime = DateTime.Now;

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
				.Get("1mKUHvBlk5wIk0LDZESO2prWvRuimhpjiWaSvoKk2gsE", "A1:2");
			headersRequest.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
			var first2Rows = headersRequest.Execute();
			var headers = first2Rows.Values[0];
			var templateRow = first2Rows.Values[1];

			var startIndex = headers.IndexOf("start");
			var versionIndex = headers.IndexOf("version");
			var secondsIndex = headers.IndexOf("seconds");
			var labelIndex = headers.IndexOf("label");
			var tooltipIndex = headers.IndexOf("tooltip");
			var agentIndex = headers.IndexOf("agent");
			var durationIndex = headers.IndexOf("duration");

			var newRow = new object[20];
			newRow[startIndex] = startTime.ToString("yyyy-MM-dd HH:mm");
			newRow[versionIndex] = typeof(SendLargeBatchesTest).Assembly.GetName().Version.ToString();
			newRow[secondsIndex] = elapsed.TotalSeconds;
			newRow[labelIndex] = templateRow[labelIndex];
			newRow[tooltipIndex] = templateRow[tooltipIndex];
			newRow[agentIndex] = Environment.MachineName;
			newRow[durationIndex] = elapsed;

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