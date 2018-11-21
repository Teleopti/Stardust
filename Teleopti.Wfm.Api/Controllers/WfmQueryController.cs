using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;

namespace Teleopti.Wfm.Api.Controllers
{
    public class WfmQueryController : ApiController
    {
		private readonly IComponentContext services;
		private readonly DtoProvider dtoProvider;
		private readonly QueryDtoProvider queryDtoProvider;
		private readonly QueryHandlerProvider handlerProvider;

        public WfmQueryController(IComponentContext services, DtoProvider dtoProvider, QueryDtoProvider queryDtoProvider, QueryHandlerProvider handlerProvider)
        {
            this.handlerProvider = handlerProvider;
            this.queryDtoProvider = queryDtoProvider;
            this.dtoProvider = dtoProvider;
            this.services = services;
        }

        [HttpGet, Route("query")]
        public IHttpActionResult AvailableQueries()
        {
	        return Json(new QueryResultDto<string>
	        {
		        Result = handlerProvider.AllowedQueryTypes().Select(t =>
			        t.Item3.Name.Substring(0, t.Item3.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase)) +
			        "/" + t.Item2.Name.Substring(0,
				        t.Item2.Name.LastIndexOf("Dto", StringComparison.InvariantCultureIgnoreCase))).ToArray(),
		        Successful = true
	        });
        }

		[HttpPost, Route("query/{queryType}/{query}")]
		public async Task<IHttpActionResult> Post(string queryType, string query)
		{
			using (var scope = services.Resolve<ILifetimeScope>())
			{
				if (!queryDtoProvider.TryFindType(query + "Dto", out var kindOfQuery))
				{
					return BadRequest("Issue a GET to /query to list available queries");
				}

				if (!dtoProvider.TryFindType(queryType + "Dto", out var type))
				{
					return BadRequest("Issue a GET to /query to list available queries");
				}

				try
				{
					var text = await Request.Content.ReadAsStringAsync();
					var value = Newtonsoft.Json.JsonConvert.DeserializeObject(text, kindOfQuery);
					var handler = scope.Resolve(typeof(IQueryHandler<,>).MakeGenericType(kindOfQuery, type));
					var method = handler.GetType().GetMethod("Handle");
					var result = method.Invoke(handler, new[] {value});
					return Json(result);
				}
				catch (Exception e)
				{
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
