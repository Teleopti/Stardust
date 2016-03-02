using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class HealthCheckApiController : ApiController
	{
		private readonly IMessagePopulatingServiceBusSender _populatingPublisher;
		private readonly IEtlJobStatusRepository _etlJobStatusRepository;
		private readonly IEtlLogObjectRepository _etlLogObjectRepository;
		private readonly StardustSender _stardustSender;
		private readonly IToggleManager _toggleManager;

		public HealthCheckApiController(IMessagePopulatingServiceBusSender populatingPublisher,
												  IEtlJobStatusRepository etlJobStatusRepository, IEtlLogObjectRepository etlLogObjectRepository,
												  StardustSender stardustSender, IToggleManager toggleManager)
		{
			_populatingPublisher = populatingPublisher;
			_etlJobStatusRepository = etlJobStatusRepository;
			_etlLogObjectRepository = etlLogObjectRepository;
			_stardustSender = stardustSender;
			_toggleManager = toggleManager;
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/CheckBus")]
		public virtual IHttpActionResult CheckBus()
		{
			var diagnosticsMessage = new DiagnosticsMessage();
			_populatingPublisher.Send(diagnosticsMessage, false);
			return Ok(new {diagnosticsMessage.InitiatorId});
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/LoadEtlJobHistory")]
		public virtual IHttpActionResult LoadEtlJobHistory([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, bool showOnlyErrors)
		{
			var etlJobStatusModels = _etlJobStatusRepository.Load(date.GetValueOrDefault(DateOnly.Today), showOnlyErrors);
			return Json(etlJobStatusModels);
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/LoadEtlLogObject")]
		public virtual IHttpActionResult LoadEtlLogObject()
		{
			var etlLogObjectModels = _etlLogObjectRepository.Load();
			return Ok(etlLogObjectModels);
		}

		[HttpGet, Route("api/HealthCheck/ServerDetails")]
		public virtual IHttpActionResult ServerDetails()
		{
			Func<Uri, Tuple<bool, string>> pingAddress = s =>
			{
				using (var client = new WebClient())
				{
					try
					{
						client.DownloadString(s);
						return new Tuple<bool, string>(true, string.Empty);
					}
					catch (Exception x)
					{
						return new Tuple<bool, string>(false, x.Message);
					}
				}
			};

			Func<IEnumerable<Tuple<string, bool, string>>> pingAllUrlsFromSettings =
				() =>
				{
					var result = new List<Tuple<string, bool, string>>();
					foreach (string key in ConfigurationManager.AppSettings.AllKeys)
					{
						string value = ConfigurationManager.AppSettings[key];

						Uri uri;
						if (Uri.TryCreate(value, UriKind.Absolute, out uri))
						{
							var pingResult = pingAddress(uri);
							result.Add(new Tuple<string, bool, string>(value, pingResult.Item1, pingResult.Item2));
						}
					}
					return result;
				};

			var computerInfo = new ServerComputer();
			var services = System.ServiceProcess.ServiceController.GetServices();
			

			return Ok(new 
			{
				Data =
					new
					{
						computerInfo.Name,
						computerInfo.Info.AvailablePhysicalMemory,
						computerInfo.Info.InstalledUICulture.DisplayName,
						computerInfo.Info.OSFullName,
						computerInfo.Info.OSPlatform,
						computerInfo.Info.OSVersion,
						computerInfo.Info.TotalPhysicalMemory,
						computerInfo.Info.TotalVirtualMemory,
						TimeZoneInfo.Local.Id,
						UrlsReachable = new
						{
							UrlResults = pingAllUrlsFromSettings().Select(s => new {Url = s.Item1,Reachable = s.Item2,Message = s.Item3})
						},
						RunningServices = new
						{
							Services = services.Where(s => s.ServiceName.Contains("Teleopti")).Select(p => new
							{
								p.DisplayName,
								p.Status
							})
						}
					}});
		}

		[HttpGet, UnitOfWork, Route("HealthCheck/CheckStardust")]
		public virtual IHttpActionResult CheckStardust()
		{
			var id = Guid.Empty;
			if(_toggleManager.IsEnabled(Toggles.Wfm_Use_Stardust))
				id = _stardustSender.Send(new StardustHealthCheckEvent(), "Stardust healthcheck", "HealthCheck",
													typeof (StardustHealthCheckEvent).ToString());
			return Ok(id);
		}
	}
}