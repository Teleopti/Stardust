using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class ToggleController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly SaveToggleOverride _saveToggleOverride;
		private readonly IFetchToggleOverride _fetchToggleOverride;

		public ToggleController(IToggleManager toggleManager, SaveToggleOverride saveToggleOverride, IFetchToggleOverride fetchToggleOverride)
		{
			_toggleManager = toggleManager;
			_saveToggleOverride = saveToggleOverride;
			_fetchToggleOverride = fetchToggleOverride;
		}

		[HttpGet, Route("Toggle/IsEnabled")]
		public IHttpActionResult IsEnabled(string toggle)
		{
			Toggles enumToggle = (Toggles)Enum.Parse(typeof(Toggles), toggle, true);

			return Json(_toggleManager.IsEnabled(enumToggle));
		}

		[HttpGet, Route("Toggle/AllOverrides")]
		public JsonResult<IEnumerable<OverrideModel>> GetAllOverrides()
		{
			return Json(_fetchToggleOverride.OverridenValues()
				.Select(x => new OverrideModel {Toggle = x.Key, Enabled = x.Value}));
		}

		[HttpPost, Route("Toggle/SaveOverride")]
		public void SaveOverride(Toggles toggle, bool value)
		{
			_saveToggleOverride.Save(toggle, value);
		}
	}

	public class OverrideModel
	{
		public string Toggle { get; set; }
		public bool Enabled { get; set; }
	}
}