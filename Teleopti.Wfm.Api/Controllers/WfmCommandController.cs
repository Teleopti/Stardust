using System;
using System.Linq;
using System.Web.Http;
using Autofac;

namespace Teleopti.Wfm.Api.Controllers
{
    [Route("api/wfm/command")]
    public class WfmCommandController : ApiController
    {
        readonly IComponentContext services;
        readonly CommandDtoProvider dtoProvider;

        public WfmCommandController(IComponentContext services, CommandDtoProvider dtoProvider)
        {
            this.dtoProvider = dtoProvider;
            this.services = services;
        }

	    [HttpGet]
	    public IHttpActionResult AvailableCommands()
	    {
		    return Json(new QueryResultDto<string>
		    {
			    Result = dtoProvider.AllowedCommandTypes().Select(t =>
				    t.Name.Substring(0, t.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase))).ToArray(),
			    Successful = true
		    });
	    }

	    [HttpPost,Route("{commandType}")]
        public IHttpActionResult Post(string commandType)
        {
			using (var scope = services.Resolve<ILifetimeScope>())
			{
				if (!dtoProvider.TryFindType(commandType + "Dto", out var type))
				{
					return BadRequest("Issue a GET to api/wfm/command to list available commands");
				}

				string text = Request.Content.ReadAsStringAsync().Result;
				var value = Newtonsoft.Json.JsonConvert.DeserializeObject(text, type);
				var handler = scope.Resolve(typeof(ICommandHandler<>).MakeGenericType(value.GetType()));
				var method = handler.GetType().GetMethod("Handle");
				var result = method.Invoke(handler, new[] {value});
				return Json(result);
			}
		}
    }
}
