using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.VisualBasic.Devices;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
    public class ApplicationController : Controller
    {
	    private readonly IServiceBusEventPopulatingPublisher _populatingPublisher;
	    private readonly IEtlJobStatusRepository _etlJobStatusRepository;
		private readonly IEtlLogObjectRepository _etlLogObjectRepository;

		public ApplicationController(IServiceBusEventPopulatingPublisher populatingPublisher, IEtlJobStatusRepository etlJobStatusRepository, IEtlLogObjectRepository etlLogObjectRepository)
	    {
		    _populatingPublisher = populatingPublisher;
		    _etlJobStatusRepository = etlJobStatusRepository;
		    _etlLogObjectRepository = etlLogObjectRepository;
	    }

	    public ViewResult Index()
        {
            return new ViewResult();
        }

	    [HttpGet, UnitOfWorkAction]
	    public ActionResult CheckBus()
	    {
		    var diagnosticsMessage = new DiagnosticsMessage();
		    _populatingPublisher.Publish(diagnosticsMessage);
		    return Json(new {diagnosticsMessage.InitiatorId},JsonRequestBehavior.AllowGet);
	    }

	    [HttpGet, UnitOfWorkAction]
	    public ActionResult LoadEtlJobHistory(DateOnly? date, bool showOnlyErrors)
	    {
		    var etlJobStatusModels = _etlJobStatusRepository.Load(date.GetValueOrDefault(DateOnly.Today), showOnlyErrors);
		    return Json(etlJobStatusModels, JsonRequestBehavior.AllowGet);
	    }

		[HttpGet, UnitOfWorkAction]
		public ActionResult LoadEtlLogObject()
		{
			var etlLogObjectModels = _etlLogObjectRepository.Load();
			return Json(etlLogObjectModels, JsonRequestBehavior.AllowGet);
		}

	    [HttpGet]
	    public ActionResult ServerDetails()
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
			

			return new JsonResult
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
					},
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
		}
    }
}
