using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class ToggleOverrideController : ApiController
	{
		private readonly IPersistToggleOverride _persistToggleOverride;
		private readonly IFetchAllToggleOverrides _fetchAllToggleOverrides;
		private readonly AllToggleNamesWithoutOverrides _allToggleNamesWithoutOverrides;
		private readonly DeleteToggleOverride _deleteToggleOverride;

		public ToggleOverrideController(IPersistToggleOverride persistToggleOverride, IFetchAllToggleOverrides fetchAllToggleOverrides, AllToggleNamesWithoutOverrides allToggleNamesWithoutOverrides, DeleteToggleOverride deleteToggleOverride)
		{
			_persistToggleOverride = persistToggleOverride;
			_fetchAllToggleOverrides = fetchAllToggleOverrides;
			_allToggleNamesWithoutOverrides = allToggleNamesWithoutOverrides;
			_deleteToggleOverride = deleteToggleOverride;
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
			_persistToggleOverride.Save(input.Toggle, input.Value);
		}

		[HttpDelete, Route("Toggle/DeleteOverride/{toggle}")]
		public void DeleteOverride(string toggle)
		{
			_deleteToggleOverride.Execute(toggle);
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