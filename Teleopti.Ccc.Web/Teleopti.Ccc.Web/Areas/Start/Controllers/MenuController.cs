using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class MenuController : ApiController
    {
		private readonly IMenuViewModelFactory _menuViewModelFactory;

		public MenuController(IMenuViewModelFactory menuViewModelFactory)
    	{
    		_menuViewModelFactory = menuViewModelFactory;
    	}

		[HttpGet, Route("Start/Menu/Applications")]
		public IHttpActionResult Applications()
		{
			return Ok(_menuViewModelFactory.CreateMenuViewModel());
		}
    }
}
