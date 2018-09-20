using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Toggle.Admin;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class ToggleController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly SaveToggleOverride _saveToggleOverride;
		private readonly FetchAllToggleOverrides _fetchAllToggleOverrides;
		private readonly AllToggleNamesWithoutOverrides _allToggleNamesWithoutOverrides;

		public ToggleController(IToggleManager toggleManager, SaveToggleOverride saveToggleOverride, FetchAllToggleOverrides fetchAllToggleOverrides,
			AllToggleNamesWithoutOverrides allToggleNamesWithoutOverrides)
		{
			_toggleManager = toggleManager;
			_saveToggleOverride = saveToggleOverride;
			_allToggleNamesWithoutOverrides = allToggleNamesWithoutOverrides;
			_fetchAllToggleOverrides = fetchAllToggleOverrides;
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
			return Json(_allToggleNamesWithoutOverrides.Execute());
		}

		[HttpGet, Route("Toggle/AllOverrides")]
		public JsonResult<IEnumerable<OverrideModel>> GetAllOverrides()
		{
			return Json(_fetchAllToggleOverrides.OverridenValues().Select(x => new OverrideModel {Toggle = x.Key, Enabled = x.Value}));
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