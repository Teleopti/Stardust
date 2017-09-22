using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ThemeController : ApiController
	{
		private readonly ISettingsPersisterAndProvider<ThemeSetting> _themeProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public ThemeController(ISettingsPersisterAndProvider<ThemeSetting> themeProvider, ILoggedOnUser loggedOnUser)
		{
			_themeProvider = themeProvider;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, HttpGet, Route("api/Theme")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_themeProvider.GetByOwner(_loggedOnUser.CurrentUser()));
		}

		[UnitOfWork, HttpPost, Route("api/Theme/Change")]
		public virtual void Change([FromBody]ThemeInput theme)
		{
			_themeProvider.Persist(new ThemeSetting
			{
				Name = theme.Name,
				Overlay = theme.Overlay
			});
		}
	}

	public class ThemeInput
	{
		public string Name { get; set; }
		public bool Overlay { get; set; }
	}
}