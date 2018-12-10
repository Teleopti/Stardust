using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class VersionController : ApiController
	{
		private readonly SystemVersion _version;

		public VersionController(SystemVersion version)
		{
			_version = version;
		}

		[Route("api/Global/Version"), HttpGet]
		public string Version() => _version.Version();
	}
}