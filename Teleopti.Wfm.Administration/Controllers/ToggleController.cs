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
		private readonly IAllToggles _allToggles;

		public ToggleController(IToggleManager toggleManager, SaveToggleOverride saveToggleOverride, IFetchToggleOverride fetchToggleOverride, IAllToggles allToggles)
		{
			_toggleManager = toggleManager;
			_saveToggleOverride = saveToggleOverride;
			_fetchToggleOverride = fetchToggleOverride;
			_allToggles = allToggles;
		}

		[HttpGet, Route("Toggle/IsEnabled")]
		public IHttpActionResult IsEnabled(string toggle)
		{
			Toggles enumToggle = (Toggles)Enum.Parse(typeof(Toggles), toggle, true);
			return Json(_toggleManager.IsEnabled(enumToggle));
		}
		
		[HttpGet, Route("Toggle/AllToggleNamesWithoutOverrides")]
		public JsonResult<IEnumerable<string>> AllToggleNamesWithoutOverrides()
		{
			var toggles = _allToggles.Toggles().Select(x=>x.ToString()).OrderBy(x=>x);
			var overrides = new HashSet<string>(_fetchToggleOverride.OverridenValues().Select(x => x.Key));
			return Json(toggles.Except(overrides));
		}

		[HttpGet, Route("Toggle/AllOverrides")]
		public JsonResult<IEnumerable<OverrideModel>> GetAllOverrides()
		{
			return Json(_fetchToggleOverride.OverridenValues().Select(x => new OverrideModel {Toggle = x.Key, Enabled = x.Value}));
		}

		[HttpPost, Route("Toggle/SaveOverride")]
		public void SaveOverride(SaveOverrideInput input)
		{
			_saveToggleOverride.Save(input.Toggle, input.Value);
		}

		[HttpDelete, Route("Toggle/DeleteOverride/{toggle}")]
		public void DeleteOverride(Toggles toggle)
		{
			_saveToggleOverride.Delete(toggle);
		}
	}

	public class SaveOverrideInput
	{
		public Toggles Toggle { get; set; }
		public bool Value { get; set; }
	}

	public class OverrideModel
	{
		public string Toggle { get; set; }
		public bool Enabled { get; set; }
	}
}