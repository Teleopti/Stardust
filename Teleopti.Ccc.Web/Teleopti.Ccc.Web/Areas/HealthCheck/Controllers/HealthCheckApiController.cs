using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions)]
	public class HealthCheckApiController : ApiController
	{
		private readonly IEtlJobStatusRepository _etlJobStatusRepository;
		private readonly IEtlLogObjectRepository _etlLogObjectRepository;
		private readonly IStardustSender _stardustSender;
		private readonly IToggleManager _toggleManager;
		private readonly HangfireUtilities _hangfireUtilities;
		private readonly IReadModelValidator _readModelValidator;
		private readonly IStardustRepository _stardustRepository;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly LatestStatisticsTimeProvider _latestStatisticsTimeProvider;

		public HealthCheckApiController(IEtlJobStatusRepository etlJobStatusRepository, IEtlLogObjectRepository etlLogObjectRepository,
												  IStardustSender stardustSender, IToggleManager toggleManager, HangfireUtilities hangfireUtilities, IReadModelValidator readModelValidator, 
												  IStardustRepository stardustRepository, IncomingTrafficViewModelCreator incomingTrafficViewModelCreator, LatestStatisticsTimeProvider latestStatisticsTimeProvider)
		{
			_etlJobStatusRepository = etlJobStatusRepository;
			_etlLogObjectRepository = etlLogObjectRepository;
			_stardustSender = stardustSender;
			_toggleManager = toggleManager;
		    _hangfireUtilities = hangfireUtilities;
			_readModelValidator = readModelValidator;
			_stardustRepository = stardustRepository;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_latestStatisticsTimeProvider = latestStatisticsTimeProvider;
		}

		[HttpGet, UnitOfWork, Route("api/HealthCheck/IncomingTrafficLatestInterval/{skillId}")]
		public virtual IHttpActionResult IncomingTrafficLatestInterval(Guid skillId)
		{
			var latestInterval = _latestStatisticsTimeProvider.Get(new[] { skillId });
			var incomingTrafficDataSeries = _incomingTrafficViewModelCreator.Load(new[] {skillId}, 0).DataSeries;

			for (var i = 0; i < incomingTrafficDataSeries.Time.Length; i++)
			{
				if (incomingTrafficDataSeries.Time[i].Hour == latestInterval.StartTime.Hour && incomingTrafficDataSeries.Time[i].Minute == latestInterval.StartTime.Minute)
				{
					return Ok(new IncomingTrafficModel
					{
						IntervalStartTime = incomingTrafficDataSeries.Time[i],
						ActualCalls = incomingTrafficDataSeries.CalculatedCalls[i],
						ForecastedCalls = incomingTrafficDataSeries.ForecastedCalls[i]
					});
				}
			}
			return Ok();
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
						if (Uri.TryCreate(value, UriKind.Absolute, out uri) && !uri.IsFile && !uri.IsUnc)
						{
							if (key == "ManagerLocation")
							{
								result.Add(new Tuple<string, bool, string>(value, false, "Skip"));
							}
							else
							{
								var pingResult = pingAddress(uri);
								result.Add(new Tuple<string, bool, string>(value, pingResult.Item1, pingResult.Item2));
							}
						}
					}
					return result;
				};

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
					servicesResult.Add(new {DisplayName = host + " Access is denied", Status = 1});
				}
				catch (Exception e)
				{
					servicesResult.Add(new {DisplayName = host + " Error: " + e.Message, Status = 1});
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
							UrlResults = pingAllUrlsFromSettings().Select(s => new {Url = s.Item1,Reachable = s.Item2,Message = s.Item3})
						},
						RunningServices = new
						{
							Services = servicesResult.ToArray()
						}
					}});
		}

		[HttpGet, UnitOfWork, Route("HealthCheck/CheckStardust")]
		public virtual IHttpActionResult CheckStardust()
		{
			var id = _stardustSender.Send(new StardustHealthCheckEvent {JobName = "Stardust healthcheck", UserName = "Health Check"});
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
			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets |= ValidateReadModelType.PersonScheduleDay;
			}

			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets |= ValidateReadModelType.ScheduleDay;
			} 

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
			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets |= ValidateReadModelType.PersonScheduleDay;
			}

			if (_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets |= ValidateReadModelType.ScheduleDay;
			} 

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
		public virtual IHttpActionResult ReinitializeReadModels(DateTime start,DateTime end)
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets |= ValidateReadModelType.PersonScheduleDay;
			}

			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets |= ValidateReadModelType.ScheduleDay;
			}

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
			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets |= ValidateReadModelType.PersonScheduleDay;
			}

			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets |= ValidateReadModelType.ScheduleDay;
			}
			_readModelValidator.ClearResult(targets);
			return Ok();
		}



		[HttpGet, Route("HealthCheck/FixScheduleProjectionReadOnly")]
		public virtual IHttpActionResult FixScheduleProjectionReadOnly()
		{
			var targets = ValidateReadModelType.ScheduleProjectionReadOnly;
			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelPersonScheduleDay_39421))
			{
				targets |= ValidateReadModelType.PersonScheduleDay;
			}

			if(_toggleManager.IsEnabled(Toggles.HealthCheck_ValidateReadModelScheduleDay_39423))
			{
				targets |= ValidateReadModelType.ScheduleDay;
			}

			var jobId = _stardustSender.Send(new FixReadModelsEvent
			{
				Targets = targets
			});
			return Ok(jobId);
		}
	}
}
