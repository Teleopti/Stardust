using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions)]
	public class HealthCheckApiController : ApiController
	{
		private readonly IEtlJobStatusRepository _etlJobStatusRepository;
		private readonly IEtlLogObjectRepository _etlLogObjectRepository;
		private readonly IStardustSender _stardustSender;
		private readonly HangfireUtilities _hangfireUtilities;
		private readonly IReadModelValidator _readModelValidator;
		private readonly IStardustRepository _stardustRepository;
		private readonly IntradayIncomingTrafficApplicationService _intradayIncomingTrafficApplicationService;

		private static readonly HttpClient client = new HttpClient();

		public HealthCheckApiController(IEtlJobStatusRepository etlJobStatusRepository, IEtlLogObjectRepository etlLogObjectRepository,
												  IStardustSender stardustSender, HangfireUtilities hangfireUtilities, IReadModelValidator readModelValidator,
												  IStardustRepository stardustRepository,
			IntradayIncomingTrafficApplicationService intradayIncomingTrafficApplicationService)
		{
			_etlJobStatusRepository = etlJobStatusRepository;
			_etlLogObjectRepository = etlLogObjectRepository;
			_stardustSender = stardustSender;
			_hangfireUtilities = hangfireUtilities;
			_readModelValidator = readModelValidator;
			_stardustRepository = stardustRepository;
			_intradayIncomingTrafficApplicationService = intradayIncomingTrafficApplicationService ?? throw new ArgumentNullException(nameof(intradayIncomingTrafficApplicationService));
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/IncomingTrafficToday/{skillId}")]
		public virtual IHttpActionResult IncomingTrafficToday(Guid skillId)
		{
			var incomingTrafficDataSeries = new IntradayIncomingDataSeries();
			incomingTrafficDataSeries = _intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { skillId }, 0).DataSeries;

			var output = incomingTrafficDataSeries.Time.Select((t, i) => new IncomingTrafficModel
			{
				IntervalStartTime = t,
				ActualCalls = incomingTrafficDataSeries.CalculatedCalls[i],
				ForecastedCalls = incomingTrafficDataSeries.ForecastedCalls[i]
			});
			return Ok(output);
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
			Tuple<bool, string> PingAddress(Uri s)
			{
				try
				{
					client.GetAsync(s).Result.EnsureSuccessStatusCode();
					return new Tuple<bool, string>(true, string.Empty);
				}
				catch (Exception x)
				{
					return new Tuple<bool, string>(false, x.Message);
				}
			}

			IEnumerable<Tuple<string, bool, string>> PingAllUrlsFromSettings()
			{
				var result = new List<Tuple<string, bool, string>>();
				foreach (string key in ConfigurationManager.AppSettings.AllKeys)
				{
					string value = ConfigurationManager.AppSettings[key];

					if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile && !uri.IsUnc)
					{
						if (key == "ManagerLocation")
						{
							result.Add(new Tuple<string, bool, string>(value, false, "Skip"));
						}
						else
						{
							var pingResult = PingAddress(uri);
							result.Add(new Tuple<string, bool, string>(value, pingResult.Item1, pingResult.Item2));
						}
					}
				}

				return result;
			}

			var computerInfo = new ServerComputer();
			var nodes = _stardustRepository.GetAllWorkerNodes();
			var hosts = nodes.Where(x => x.Alive).Select(x => x.Url.Host).Distinct().ToList();
			var servicesResult = new List<object>();
			foreach (var host in hosts)
			{
				try
				{
					var remoteServices = ServiceController.GetServices(host);
					var teleoptServices = remoteServices.Where(s => s.ServiceName.Contains("Teleopti")).Select(p => new
					{
						DisplayName = host + " " + p.DisplayName,
						p.Status
					}).ToArray();
					servicesResult.AddRange(teleoptServices);
				}
				catch (InvalidOperationException ioe) when (ioe.InnerException is Win32Exception &&
															ioe.InnerException.Message.Contains("Access is denied"))
				{
					servicesResult.Add(new { DisplayName = host + " Access is denied", Status = 1 });
				}
				catch (Exception e)
				{
					servicesResult.Add(new { DisplayName = host + " Error: " + e.Message, Status = 1 });
				}
			}

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
							UrlResults = PingAllUrlsFromSettings().Select(s => new { Url = s.Item1, Reachable = s.Item2, Message = s.Item3 })
						},
						RunningServices = new
						{
							Services = servicesResult.ToArray()
						}
					}
			});
		}

		[HttpGet, UnitOfWork, Route("HealthCheck/CheckStardust")]
		public virtual IHttpActionResult CheckStardust()
		{
			var id = _stardustSender.Send(new StardustHealthCheckEvent { JobName = "Stardust healthcheck", UserName = "Health Check" });
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
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			targets |= ValidateReadModelType.PersonScheduleDay;

			targets |= ValidateReadModelType.ScheduleDay;

			var jobId = _stardustSender.Send(new ValidateReadModelsEvent
			{
				StartDate = start,
				EndDate = end,
				Targets = targets
			});
			return Ok(jobId);
		}

		[HttpGet, Route("HealthCheck/CheckAndFixReadModels")]
		public virtual IHttpActionResult CheckAndFixReadModels(DateTime start, DateTime end)
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			targets |= ValidateReadModelType.PersonScheduleDay;
			targets |= ValidateReadModelType.ScheduleDay;

			var jobId = _stardustSender.Send(new ValidateReadModelsEvent
			{
				StartDate = start,
				EndDate = end,
				Targets = targets,
				TriggerFix = true
			});
			return Ok(jobId);
		}

		[HttpGet, Route("HealthCheck/ReinitializeReadModels")]
		public virtual IHttpActionResult ReinitializeReadModels(DateTime start, DateTime end)
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			targets |= ValidateReadModelType.PersonScheduleDay;
			targets |= ValidateReadModelType.ScheduleDay;

			var jobId = _stardustSender.Send(new ValidateReadModelsEvent
			{
				StartDate = start,
				EndDate = end,
				Targets = targets,
				Reinitialize = true
			});
			return Ok(jobId);
		}


		[HttpGet, Route("HealthCheck/ClearCheckReadModelResult")]
		public virtual IHttpActionResult ClearCheckReadModelResult()
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			targets |= ValidateReadModelType.PersonScheduleDay;
			targets |= ValidateReadModelType.ScheduleDay;
			_readModelValidator.ClearResult(targets);
			return Ok();
		}



		[HttpGet, Route("HealthCheck/FixScheduleProjectionReadOnly")]
		public virtual IHttpActionResult FixScheduleProjectionReadOnly()
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			targets |= ValidateReadModelType.PersonScheduleDay;
			targets |= ValidateReadModelType.ScheduleDay;

			var jobId = _stardustSender.Send(new FixReadModelsEvent
			{
				Targets = targets
			});
			return Ok(jobId);
		}
	}
}
