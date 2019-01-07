using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using log4net;

namespace Teleopti.Wfm.Api.Controllers
{
    public class WfmCommandController : ApiController
    {
        private readonly IComponentContext services;
        private readonly CommandDtoProvider dtoProvider;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(WfmCommandController));

        public WfmCommandController(IComponentContext services, CommandDtoProvider dtoProvider)
        {
            this.dtoProvider = dtoProvider;
            this.services = services;
        }

	    [HttpGet, Route("command")]
	    public IHttpActionResult AvailableCommands()
	    {
		    return Json(new QueryResultDto<string>
		    {
			    Result = dtoProvider.AllowedCommandTypes().Select(t =>
				    t.Name.Substring(0, t.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase))).ToArray(),
			    Successful = true
		    });
	    }

		[HttpPost, Route("command/{commandType}")]
		public async Task<IHttpActionResult> Post(string commandType)
		{
			using (var scope = services.Resolve<ILifetimeScope>())
			{
				if (!dtoProvider.TryFindType(commandType + "Dto", out var type))
				{
					return BadRequest("Issue a GET to /command to list available commands");
				}
				try
				{
					var text = await Request.Content.ReadAsStringAsync();
					var value = Newtonsoft.Json.JsonConvert.DeserializeObject(text, type);
					var handler = scope.Resolve(typeof(ICommandHandler<>).MakeGenericType(value.GetType()));
					var method = handler.GetType().GetMethod("Handle");
					var result = method.Invoke(handler, new[] {value});
					return Json(result);
				}
				catch (Exception e)
				{
					Logger.Warn($"Error when running command {commandType}.", e);
					return Json(new ResultDto
					{
						Successful = false,
						Message = e.Message
					});
				}
			}
		}
	}
}
