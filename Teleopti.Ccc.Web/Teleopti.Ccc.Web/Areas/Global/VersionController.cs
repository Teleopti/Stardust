using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class VersionController : ApiController
	{
		private readonly SystemVersion _version;
		private readonly IToggleManager _toggles;

		public VersionController(SystemVersion version, IToggleManager toggles)
		{
			_version = version;
			_toggles = toggles;
		}

		[Route("api/Global/Version"), HttpGet]
		public string Version() => _version.Version();
		
	}
}