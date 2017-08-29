using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;

namespace Teleopti.Ccc.TestCommon.PerformanceTest
{


	// Use this query if you want to extract performance data from team city
	
	//	--select 
	//	--replace(test_name, 'Teleopti.Ccc.Rta.PerformanceTest.dll: ', '') name,
	//	--* 
	//	--from
	//	--test_names 
	//	--where test_name like '%Rta.PerformanceTest%' 
	//	--order by
	//	--name

	//	select
	//	convert(varchar, dateadd(S, cast(h.build_start_time_server/1000 as int), '1970-01-01'), 120) start,
	//	substring(h.build_number, charindex(' - ', h.build_number) + 3, 100) [version],
	//convert(varchar, cast(t.duration as float)/1000, 128) seconds,
	//'' label,
	//'' tooltip,
	//h.agent_name agent,
	//convert(varchar, dateadd(ms, t.duration, 0), 108) duration,
	//'Extracted from team city test ' + replace(n.test_name, 'Teleopti.Ccc.Rta.PerformanceTest.dll: ', '') comment
	//from
	//test_info t
	//left join test_names n on
	//n.id = t.test_name_id
	//left join history h on
	//h.build_id = t.build_id
	//where
	//n.test_name like '%Rta.PerformanceTest%'
	//order by
	//comment,
	//start


	public class PerformanceTest
	{
		public PerformanceTest()
		{
		}
		
		public void Measure(string spreadsheetId, string sheetName, Action action)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var startTime = DateTime.Now;

			action.Invoke();

			stopwatch.Stop();
			appendResult(startTime, stopwatch.Elapsed, spreadsheetId, sheetName);
		}

		private static void appendResult(DateTime startTime, TimeSpan elapsed, string spreadsheetId, string sheetName)
		{
			var json = File.ReadAllText("serviceaccount.json");
			var serviceaccount_json = JsonConvert.DeserializeObject<serviceaccount_json>(json);

			var initializer = new ServiceAccountCredential.Initializer(serviceaccount_json.client_email)
			{
				Scopes = new[] {SheetsService.Scope.Spreadsheets}
			};
			initializer = initializer.FromPrivateKey(serviceaccount_json.private_key);
			var credential = new ServiceAccountCredential(initializer);

			var service = new SheetsService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "Google Sheets API",
			});

			var headersRequest = service.Spreadsheets.Values
				.Get(
					spreadsheetId,
					$"{sheetName}!A1:2"
				);
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
			var commentIndex = headers.IndexOf("comment");

			var newRow = new object[20];
			newRow[startIndex] = startTime.ToString("yyyy-MM-dd HH:mm");
			newRow[versionIndex] = typeof(PerformanceTest).Assembly.GetName().Version.ToString();
			newRow[secondsIndex] = elapsed.TotalSeconds;
			newRow[labelIndex] = templateRow[labelIndex];
			newRow[tooltipIndex] = templateRow[tooltipIndex];
			newRow[agentIndex] = Environment.MachineName;
			newRow[durationIndex] = elapsed;
			newRow[commentIndex] = templateRow[commentIndex]; ;

			var appendRequest = service.Spreadsheets.Values
				.Append(new ValueRange
					{
						Values = new List<IList<object>>
						{
							newRow
						}
					},
					spreadsheetId,
					$"{sheetName}!A1");
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}

		private class serviceaccount_json
		{
			public string client_email { get; set; }
			public string private_key { get; set; }
		}
	}
}