﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public class ReportProvider : IReportProvider
	{
		public const string TemplateReportName = "__WFM_Insights_Report_Template__";
		public const string UsageReportName = "Report Usage Metrics Report";
		private readonly IApplicationConfigurationDbProvider _appConfig;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPowerBiClientFactory _powerBiClientFactory;
		private readonly IInsightsReportRepository _reportRepository;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReportProvider));

		public ReportProvider(IApplicationConfigurationDbProvider appConfig,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IPowerBiClientFactory powerBiClientFactory,
			IInsightsReportRepository reportRepository,
			ICommonAgentNameProvider commonAgentNameProvider)
		{
			_appConfig = appConfig;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_powerBiClientFactory = powerBiClientFactory;
			_reportRepository = reportRepository;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		public async Task<ReportModel[]> GetReports()
		{
			var wfmReports = _reportRepository.GetAllValidReports();
			var reportInformation = wfmReports.ToDictionary(rep => rep.Id.GetValueOrDefault(), mapReportMetaData);

			var excludedReports = new[] { UsageReportName, TemplateReportName };
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				return reports.Value.Where(r => !excludedReports.Contains(r.Name))
					.OrderBy(r => r.Name)
					.Select(x =>
					{
						Guid.TryParse(x.Id, out var reportId);
						var reportInfo = reportInformation.ContainsKey(reportId) ? reportInformation[reportId] : null;
						return new ReportModel
						{
							Id = x.Id,
							Name = reportInfo != null ? reportInfo.Name : x.Name,
							EmbedUrl = x.EmbedUrl,
							CreatedBy = reportInfo?.CreatedBy,
							CreatedOn = reportInfo?.CreatedOn,
							UpdatedBy = reportInfo?.UpdatedBy,
							UpdatedOn = reportInfo?.UpdatedOn
						};
					})
					.ToArray();
			}
		}

		public async Task<EmbedReportConfig> GetReportConfig(Guid reportId, ReportViewMode viewMode)
		{
			var result = new EmbedReportConfig();
			var rawReportData = mapReportMetaData(_reportRepository.Get(reportId));

			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var report = reports.Value.FirstOrDefault(r => r.Id == reportId.ToString().ToLower());

				if (report == null)
				{
					logger.Error($"Group with Id \"{groupId}\" has no reports.");
					return result;
				}

				result = await generateEmbedReportConfig(client, report, viewMode);
			}

			updateReportMetaData(rawReportData, result);

			return result;
		}

		public async Task<EmbedReportConfig> CreateReport(string newReportName)
		{
			var result = new EmbedReportConfig();

			if (!isValidReportName(newReportName))
			{
				return result;
			}

			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var templateReport = reports.Value.SingleOrDefault(r => r.Name == TemplateReportName);
				if (templateReport == null)
				{
					logger.Error($"Template report \"{TemplateReportName}\" not found.");
					return result;
				}

				var newReport = client.Reports.CloneReportInGroup(groupId, templateReport.Id,
					new CloneReportRequest(newReportName));

				result = await generateEmbedReportConfig(client, newReport, ReportViewMode.Edit);
			}

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_reportRepository.AddNewInsightsReport(Guid.Parse(result.ReportId), result.ReportName);
				uow.PersistAll();
			}

			return result;
		}

		public Task<bool> UpdateReportMetadata(Guid reportId, string reportName)
		{
			try
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					_reportRepository.UpdateInsightsReport(reportId, reportName);
					uow.PersistAll();
				}

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				logger.Error($"Failed to update report raw data with Id=\"{reportId}\" and name = \"{reportName}\"", ex);
				return Task.FromResult(false);
			}
		}

		public async Task<EmbedReportConfig> CloneReport(Guid reportId, string newReportName)
		{
			var result = new EmbedReportConfig();

			var reportIdStr = reportId.ToString().ToLower();
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				var groupId = getPowerBiGroupId();
				var reports = await client.Reports.GetReportsInGroupAsync(groupId);

				var report = reports.Value.FirstOrDefault(r => r.Id == reportIdStr);

				if (report == null)
				{
					logger.Error($"Group with Id \"{groupId}\" has no reports.");
					return result;
				}

				var newReport = client.Reports.CloneReportInGroup(groupId, reportIdStr,
					new CloneReportRequest(newReportName));

				result = await generateEmbedReportConfig(client, newReport, ReportViewMode.Edit);
			}

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_reportRepository.AddNewInsightsReport(Guid.Parse(result.ReportId), result.ReportName);
				uow.PersistAll();
			}

			return result;
		}

		public async Task<bool> DeleteReport(Guid reportId)
		{
			using (var client = await _powerBiClientFactory.CreatePowerBiClient())
			{
				try
				{
					var groupId = getPowerBiGroupId();
					client.Reports.DeleteReport(groupId, reportId.ToString().ToLower());
				}
				catch (Exception ex)
				{
					logger.Error($"Failed to delete report with Id=\"{reportId}\"", ex);
					return false;
				}
			}

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_reportRepository.DeleteInsightsReport(reportId);
				uow.PersistAll();
			}

			return true;
		}

		private bool isValidReportName(string newReportName)
		{
			return !string.IsNullOrEmpty(newReportName);
		}

		private async Task<EmbedReportConfig> generateEmbedReportConfig(IPowerBIClient client,
			Report report, ReportViewMode viewMode, string userName = null, string roles = null)
		{
			var allowSaveAs = viewMode == ReportViewMode.Edit;

			GenerateTokenRequest generateTokenRequestParameters;
			if (!string.IsNullOrEmpty(userName))
			{
				var rls = new EffectiveIdentity(userName, new List<string> { report.DatasetId });
				if (!string.IsNullOrWhiteSpace(roles))
				{
					rls.Roles = roles.Split(',').ToList();
				}

				// Refer to https://github.com/Microsoft/PowerBI-CSharp/blob/master/sdk/PowerBI.Api/Source/V2/Models/GenerateTokenRequest.cs
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: viewMode.ToString(),
					allowSaveAs: allowSaveAs, identities: new List<EffectiveIdentity> {rls});
			}
			else
			{
				generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: viewMode.ToString(),
					allowSaveAs: allowSaveAs);
			}

			var groupId = getPowerBiGroupId();
			EmbedToken token;
			try
			{
				token = await client.Reports.GenerateTokenInGroupAsync(groupId, report.Id,
					generateTokenRequestParameters);
			}
			catch (Exception ex)
			{
				token = null;
				logger.Error("Error occurred on generating embed token.", ex);
			}

			if (token == null)
			{
				logger.Error("Failed to generate embed token.");
				return new EmbedReportConfig();
			}

			// The expiration got from PowerBI is earlier than now, set to Minimum Access Token Lifetime(10 minutes)
			// Refer to https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-configurable-token-lifetimes#configurable-token-lifetime-properties
			var tenMinutesLater = DateTime.Now.AddMinutes(10);
			var expiration = token.Expiration < tenMinutesLater
				? tenMinutesLater
				: token.Expiration;

			return new EmbedReportConfig
			{
				ReportId = report.Id,
				ReportName = report.Name,
				ReportUrl = report.EmbedUrl,
				TokenType = "Embed",
				AccessToken = token.Token,
				Expiration = expiration
			};
		}

		private string getPowerBiGroupId()
		{
			return _appConfig.GetTenantValue(TenantApplicationConfigKey.InsightsPowerBIGroupId);
		}

		private reportMetaData mapReportMetaData(IInsightsReport report)
		{
			if (report == null)
			{
				return null;
			}

			var nameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			return new reportMetaData
			{
				Name = report.Name,
				CreatedBy = report.CreatedBy != null ? nameSetting.BuildFor(report.CreatedBy) : null,
				CreatedOn = report.CreatedOn.GetValueOrDefault(),
				UpdatedBy = report.UpdatedBy != null ? nameSetting.BuildFor(report.UpdatedBy) : null,
				UpdatedOn = report.UpdatedOn.GetValueOrDefault()
			};
		}

		private static void updateReportMetaData(reportMetaData rawReportData, EmbedReportConfig reportConfig)
		{
			if (rawReportData == null)
			{
				return;
			}

			reportConfig.ReportName = rawReportData.Name;
			reportConfig.CreatedBy = rawReportData.CreatedBy;
			reportConfig.CreatedOn = rawReportData.CreatedOn;
			reportConfig.UpdatedBy = rawReportData.UpdatedBy;
			reportConfig.UpdatedOn = rawReportData.UpdatedOn;
		}

		private class reportMetaData
		{
			public string Name { get; set; }
			public string CreatedBy { get; set; }
			public DateTime CreatedOn { get; set; }
			public string UpdatedBy { get; set; }
			public DateTime UpdatedOn { get; set; }
		}
	}
}