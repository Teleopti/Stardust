using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Stardust;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class StardustController : ApiController
	{
		private readonly IStardustRepository _stardustRepository;
		private readonly IStardustSender _stardustSender;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly IPingNode _pingNode;
		private readonly IConfigReader _configReader;
		private readonly ISkillForecastJobStartTimeRepository _skillForecastJobStartTimeRepository;


		public StardustController(IStardustRepository stardustRepository, IStardustSender stardustSender,
			ILoadAllTenants loadAllTenants, IStaffingSettingsReader staffingSettingsReader, IPingNode pingNode, ISkillForecastJobStartTimeRepository skillForecastJobStartTimeRepository,IConfigReader configReader = null)
		{
			_stardustRepository = stardustRepository;
			_stardustSender = stardustSender;
			_loadAllTenants = loadAllTenants;
			_staffingSettingsReader = staffingSettingsReader;
			_pingNode = pingNode;
			_skillForecastJobStartTimeRepository = skillForecastJobStartTimeRepository;
			_configReader = configReader ?? new ConfigReader();
		}

		[HttpGet, Route("Stardust/Jobs")]
		public IHttpActionResult JobHistoryFiltered(int from, int to, string dataSource = null, string type = null, DateTime? fromDate = null, DateTime? toDate = null)
		{
			return Ok(_stardustRepository.GetJobs(new JobFilterModel { DataSource = dataSource, Type = type, From = from, To = to, FromDate = fromDate, ToDate = toDate }));
		}

		[HttpGet, Route("Stardust/FailedJobs")]
		public IHttpActionResult FailedJobHistoryFiltered(int from, int to, string dataSource = null, string type = null, DateTime? fromDate = null, DateTime? toDate = null)
		{
			return Ok(_stardustRepository.GetJobs(new JobFilterModel { DataSource = dataSource, Type = type, From = from, To = to, FromDate = fromDate, ToDate = toDate, Result = "Failed" }));
		}

		[HttpGet, Route("Stardust/JobsByNode/{nodeId}/{from}/{to}")]
		public IHttpActionResult JobHistoryList(Guid nodeId, int from, int to)
		{
			return Ok(_stardustRepository.GetJobsByNodeId(nodeId, from, to));
		}

		[HttpGet, Route("Stardust/JobDetails/{jobId}")]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			return Ok(_stardustRepository.GetJobDetailsByJobId(jobId));
		}

		[HttpGet, Route("Stardust/Job/{jobId}")]
		public IHttpActionResult Job(Guid jobId)
		{
			return Ok(_stardustRepository.GetJobByJobId(jobId));
		}

		[HttpGet, Route("Stardust/QueuedJobs")]
		public IHttpActionResult JobQueueFiltered(int from, int to, string dataSource = null, string type = null)
		{
			return Ok(_stardustRepository.GetAllQueuedJobs(new JobFilterModel { DataSource = dataSource, Type = type, From = from, To = to }));
		}

		[HttpGet, Route("Stardust/QueuedJobs/{jobId}")]
		public IHttpActionResult QueuedJob(Guid jobId)
		{
			return Ok(_stardustRepository.GetQueuedJob(jobId));
		}

		[HttpGet, Route("Stardust/AliveWorkerNodes")]
		public IHttpActionResult AliveWorkerNodes()
		{
			var allNodes = _stardustRepository.GetAllWorkerNodes();
			return Ok(allNodes.Where(x => x.Alive));
		}

		[HttpGet, Route("Stardust/WorkerNodes")]
		public IHttpActionResult WorkerNodes()
		{
			return Ok(_stardustRepository.GetAllWorkerNodes());
		}

		[HttpGet, Route("Stardust/WorkerNode/{id}")]
		public IHttpActionResult WorkerNodes(Guid id)
		{
			return Ok(_stardustRepository.WorkerNode(id));
		}

		[HttpPost, Route("Stardust/DeleteQueuedJobs")]
		public IHttpActionResult DeleteQueuedJobs(Guid[] ids)
		{
			_stardustRepository.DeleteQueuedJobs(ids);
			return Ok();
		}

		[HttpGet, Route("Stardust/Types")]
		public IHttpActionResult JobTypes()
		{
			return Ok(_stardustRepository.GetAllTypes());
		}

		[HttpGet, Route("Stardust/QueueTypes")]
		public IHttpActionResult QueueTypes()
		{
			return Ok(_stardustRepository.GetAllTypesInQueue());
		}

		[HttpGet, Route("Stardust/OldestJob")]
		public IHttpActionResult GetOldestJob()
		{
			return Ok(_stardustRepository.GetOldestJob());
		}

		[HttpGet, Route("Stardust/QueueCount")]
		public IHttpActionResult GetQueueCount()
		{
			return Ok(_stardustRepository.GetQueueCount());
		}

		[HttpPost, Route("Stardust/TriggerResourceCalculation")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult TriggerResourceCalculation([FromBody] LogOnModel logOnModel)
		{
			if (logOnModel == null) return BadRequest("logOnModel is null!");
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(logOnModel.Tenant));
			var appConnstring = tenant.DataSourceConfiguration.ApplicationConnectionString;
			var bus = _stardustRepository.SelectAllBus(appConnstring);

			foreach (var bu in bus)
			{
				_stardustSender.Send(
					new UpdateStaffingLevelReadModelEvent
					{
						Days = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14),
						LogOnDatasource = logOnModel.Tenant,
						LogOnBusinessUnitId = bu
					});
			}

			return Ok();
		}

		[HttpPost, Route("Stardust/TriggerSkillForecastCalculation")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult TriggerSkillForecastCalculation([FromBody] SkillForecastCalculationModel model)
		{
			if (model == null) return BadRequest("model is null!");
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(model.Tenant));
			var appConnstring = tenant.DataSourceConfiguration.ApplicationConnectionString;
			var bus = _stardustRepository.SelectAllBus(appConnstring);

			foreach (var bu in bus)
			{
				_skillForecastJobStartTimeRepository.ResetLock(bu, appConnstring);
				_stardustSender.Send(
					new UpdateSkillForecastReadModelEvent
					{
						StartDateTime = model.StartDate.Date,
						EndDateTime = model.EndDate.Date,
						LogOnDatasource = model.Tenant,
						LogOnBusinessUnitId = bu
					});
			}

			return Ok();
		}

		[HttpGet, Route("Stardust/HealthCheck")]
		public IHttpActionResult HealthCheck()
		{
			var allNodes = _stardustRepository.GetAllWorkerNodes();
			if (!allNodes.Any())
				return Ok("No nodes registered! Make sure that the Teleopti Service Bus service is running.");
			if (!allNodes.Any(x => x.Alive))
				return Ok("No node is sending heartbeats. Make sure that the Teleopti Service Bus service is running.");

			foreach (var node in allNodes.Where(x => x.Alive))
			{
				bool result;
				try
				{
					result = _pingNode.Ping(node);
				}
				catch (Exception)
				{
					return Ok($"Node {node.Url} does not respond. Make sure that the Teleopti Service Bus service is running. Is the firewall configured so the worker server allows incoming traffic on ports 14100-14199?");
				}
				if (!result)
					return Ok($"Node {node.Url} does not respond. Make sure that the Teleopti Service Bus service is running. Is the firewall configured so the worker server allows incoming traffic on ports 14100-14199?");
			}

			var dummyBUId = Guid.NewGuid();
			var id = _stardustSender.Send(new StardustHealthCheckEvent { JobName = "Stardust Health Check", UserName = "Stardust", LogOnDatasource = "Health Check", LogOnBusinessUnitId = dummyBUId });
			var waiting = true;
			var healthCheckJob = new Job();
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while (waiting)
			{
				healthCheckJob = _stardustRepository.GetJobByJobId(id);
				if (healthCheckJob.Ended != null || stopwatch.Elapsed > TimeSpan.FromSeconds(15))
					waiting = false;
				Thread.Sleep(TimeSpan.FromMilliseconds(500));
			}
			if (healthCheckJob.Ended != null && healthCheckJob.Result != "Success")
				return Ok("The Health Check job failed during execution. Check the Failed Jobs tab for more information.");
			var queuedJobs = _stardustRepository.GetAllQueuedJobs(new JobFilterModel { From = 0, To = 5 });
			if (healthCheckJob.Ended == null || queuedJobs.Any())
				return Ok("Something is wrong with Stardust and it smells like a bug!");
			return Ok("Everything looks OK!");
		}

		[HttpPost, Route("Stardust/RefreshPayrollFormats")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult RefreshPayrollFormats([FromBody] LogOnModel logOnModel)
		{
			if (logOnModel == null) return BadRequest("logOnModel is null!");
			//if (string.IsNullOrEmpty(_configReader.AppConfig("IsContainer"))) return BadRequest("");
			//var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(logOnModel.Tenant));

			_stardustSender.Send(
					new RefreshPayrollFormatsEvent
					{
						LogOnBusinessUnitId = Guid.NewGuid(),
						TenantName = logOnModel.Tenant,
						LogOnDatasource = logOnModel.Tenant
					});

			return Ok("");
		}

		[HttpGet, Route("Stardust/IntradayToolGoWithTheFlow")]
		public IHttpActionResult IntradayToolGoWithTheFlow()
		{
			var showIntradayTool = _configReader.AppConfig("ShowIntradayTool");
			if (string.IsNullOrEmpty(showIntradayTool) || showIntradayTool != "true") return BadRequest("");

			_stardustSender.Send(
				new IntradayToolEvent
				{
					LogOnBusinessUnitId = Guid.NewGuid()
				});

			return Ok("");
		}

		[HttpGet, Route("Stardust/ShowIntradayTool")]
		public IHttpActionResult ShowIntradayTool()
		{
			var showIntradayTool = _configReader.AppConfig("ShowIntradayTool");
			return Ok(!string.IsNullOrEmpty(showIntradayTool) && showIntradayTool == "true");
		}
	}
}
