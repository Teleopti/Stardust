using Microsoft.AspNetCore.Mvc;

namespace StaffHubPoC.Controllers
{
	[Route("api/[controller]")]
	public class ScheduleChangeController : Controller
    {
		[HttpPost]
		public void Post([FromBody]string value)
		{
		}
	}
}