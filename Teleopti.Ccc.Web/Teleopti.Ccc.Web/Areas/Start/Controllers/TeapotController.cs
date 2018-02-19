using System;
using System.Web.Http;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class TeapotController : ApiController
	{
		private readonly ResourceVersion _version;

		public TeapotController(ResourceVersion version)
		{
			_version = version;
		}

		[HttpGet, Route("api/Teapot/MakeBadCoffee")]
		public virtual IHttpActionResult MakeBadCoffee()
		{
			if (_version.TeapotVersion() == "2")
				throw new NotImplementedException();
			return Ok("Bad coffee");
		}

		[HttpGet, Route("api/Teapot/MakeGoodCoffee")]
		public virtual IHttpActionResult MakeGoodCoffee()
		{
			if (_version.TeapotVersion() == "2")
				return Ok("I'm a teapot");
			throw new NotImplementedException();
		}
	}
}