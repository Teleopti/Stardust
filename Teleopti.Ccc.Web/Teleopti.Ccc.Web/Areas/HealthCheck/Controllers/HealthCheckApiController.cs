using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class HealthCheckApiController : ApiController
	{
		private readonly IMessagePopulatingServiceBusSender _populatingPublisher;
		private readonly IEtlJobStatusRepository _etlJobStatusRepository;
		private readonly IEtlLogObjectRepository _etlLogObjectRepository;
		private readonly IStardustSender _stardustSender;
		private readonly IToggleManager _toggleManager;
		private readonly IHangfireUtilities _hangfireUtilities;
		private readonly IEventPublisher _publisher;

		public HealthCheckApiController(IMessagePopulatingServiceBusSender populatingPublisher,
												  IEtlJobStatusRepository etlJobStatusRepository, IEtlLogObjectRepository etlLogObjectRepository,
												  IStardustSender stardustSender, IToggleManager toggleManager, IHangfireUtilities hangfireUtilities, IEventPublisher publisher)
		{
			_populatingPublisher = populatingPublisher;
			_etlJobStatusRepository = etlJobStatusRepository;
			_etlLogObjectRepository = etlLogObjectRepository;
			_stardustSender = stardustSender;
			_toggleManager = toggleManager;
		    _hangfireUtilities = hangfireUtilities;
			_publisher = publisher;
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/CheckBus")]
		public virtual IHttpActionResult CheckBus()
		{
			var diagnosticEvent = new ServiceBusHealthCheckEvent();
			_populatingPublisher.Send(diagnosticEvent, false);

			return Ok(new {diagnosticEvent.InitiatorId});
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
				id = _stardustSender.Send(new StardustHealthCheckEvent {JobName = "Stardust healthcheck" ,UserName = "Health Check"});
			return Ok(id);
		}

		[HttpGet, UnitOfWork, Route("HealthCheck/CheckHangfireFailedQueue")]
		public virtual IHttpActionResult CheckHangfireFailedQueue()
		{
			var failedCount = _hangfireUtilities.NumberOfFailedJobs();
			return Ok(failedCount);
		}


		[HttpGet, Route("HealthCheck/CheckReadModels")]
		public virtual IHttpActionResult CheckReadModels(DateTime start, DateTime end)
		{
			var targets = new List<ValidateReadModelType>();
			targets.Add(ValidateReadModelType.ScheduleProjectionReadOnly);
			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets.Add(ValidateReadModelType.PersonScheduleDay);
			}

			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets.Add(ValidateReadModelType.ScheduleDay);
			} 

			var jobId = _stardustSender.Send(new ValidateReadModelsEvent
			{
				StartDate = start,
				EndDate = end,
				Targets = targets
			});
			return Ok(jobId);
		}

		[HttpGet, Route("HealthCheck/FixScheduleProjectionReadOnly")]
		public virtual IHttpActionResult FixScheduleProjectionReadOnly()
		{
			var targets = new List<ValidateReadModelType>();
			targets.Add(ValidateReadModelType.ScheduleProjectionReadOnly);
			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets.Add(ValidateReadModelType.PersonScheduleDay);
			}

			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets.Add(ValidateReadModelType.ScheduleDay);
			}

			var jobId = _stardustSender.Send(new FixReadModelsEvent
			{
				Targets = targets
			});
			return Ok(jobId);
		}
	}
}
