using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core.PowerBi
{
	public class FakeReports : IReports
	{
		public string Token { get; private set; }
		public Dictionary<string, List<Report>> ReportGroups { get; } = new Dictionary<string, List<Report>>();

		public void SetAccessToken(string token)
		{
			Token = token;
		}

		public void AddReports(string groupId, params Report[] newReports)
		{
			ReportGroups.TryGetValue(groupId, out var reports);
			if (reports != null)
			{
				reports.AddRange(newReports);
			}
			else
			{
				ReportGroups.Add(groupId, newReports.ToList());
			}
		}

		public Task<HttpOperationResponse<ODataResponseListReport>> GetReportsWithHttpMessagesAsync(
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<Report>> GetReportWithHttpMessagesAsync(string reportKey,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<object>> DeleteReportWithHttpMessagesAsync(string reportKey,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<Report>> CloneReportWithHttpMessagesAsync(string reportKey,
			CloneReportRequest requestParameters, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<Stream>> ExportReportWithHttpMessagesAsync(string reportKey,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<Report>> UpdateReportContentWithHttpMessagesAsync(string reportKey,
			UpdateReportContentRequest requestParameters, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<object>> RebindReportWithHttpMessagesAsync(string reportKey,
			RebindReportRequest requestParameters, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<EmbedToken>> GenerateTokenForCreateWithHttpMessagesAsync(
			GenerateTokenRequest requestParameters, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<EmbedToken>> GenerateTokenWithHttpMessagesAsync(string reportKey,
			GenerateTokenRequest requestParameters, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public async Task<HttpOperationResponse<ODataResponseListReport>> GetReportsInGroupWithHttpMessagesAsync(
			string groupId, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			ReportGroups.TryGetValue(groupId, out var reports);
			var reportList = new ODataResponseListReport
			{
				Value = reports ?? new List<Report>()
			};
			var result = new HttpOperationResponse<ODataResponseListReport> {Body = reportList};
			return await Task.FromResult(result);
		}

		public Task<HttpOperationResponse<Report>> GetReportInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<object>> DeleteReportInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return null;
		}

		public async Task<HttpOperationResponse<Report>> CloneReportInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, CloneReportRequest requestParameters,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var reports = ReportGroups[groupId];
			var sourceReport = reports.Single(x => x.Id == reportKey);
			var newReportId = Guid.NewGuid().ToString();
			var newReport = new Report
			{
				Id = newReportId,
				Name = sourceReport.Name + " - Copy",
				DatasetId = sourceReport.DatasetId,
				WebUrl = "NewWebUrl",
				EmbedUrl = "NewEmbedUrl"
			};

			reports.Add(newReport);
			
			var result = new HttpOperationResponse<Report> {Body = newReport};
			return await Task.FromResult(result);
		}

		public Task<HttpOperationResponse<Stream>> ExportReportInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<Report>> UpdateReportContentInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, UpdateReportContentRequest requestParameters,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<object>> RebindReportInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, RebindReportRequest requestParameters,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<EmbedToken>> GenerateTokenForCreateInGroupWithHttpMessagesAsync(
			string groupId, GenerateTokenRequest requestParameters,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public async Task<HttpOperationResponse<EmbedToken>> GenerateTokenInGroupWithHttpMessagesAsync(string groupId,
			string reportKey, GenerateTokenRequest requestParameters,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var token = new EmbedToken {Token = Token};
			var result = new HttpOperationResponse<EmbedToken> {Body = token};
			return await Task.FromResult(result);
		}

		public Task<HttpOperationResponse<ODataResponseListReport>> GetReportsInGroupAsAdminWithHttpMessagesAsync(
			string groupId, string filter = null, int? top = null, int? skip = null,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<HttpOperationResponse<ODataResponseListReport>> GetReportsAsAdminWithHttpMessagesAsync(
			string filter = null, int? top = null, int? skip = null,
			Dictionary<string, List<string>> customHeaders = null,
			CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}
	}
}