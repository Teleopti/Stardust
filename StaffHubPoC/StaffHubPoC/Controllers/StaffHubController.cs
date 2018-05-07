using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace StaffHubPoC.Controllers
{
	public class StaffHubController : ApiController
	{
		[HttpGet, Route("api/staffhub/teams")]
		public IHttpActionResult Teams()
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://api.manage.staffhub.office.com/");
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var result = client.GetAsync("api/beta/users/me/teams").Result;
				if (result.IsSuccessStatusCode)
					return Ok();
				return BadRequest("Nope.");
			}

			//// GET: StaffHub
			//public ActionResult Index()
			//{
			//    return View();
			//}
		}
	}
}