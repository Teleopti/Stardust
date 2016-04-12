using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ThemeController : ApiController
	{
		private readonly ThemeSettingProvider _themeProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public ThemeController(ThemeSettingProvider themeProvider, ILoggedOnUser loggedOnUser)
		{
			_themeProvider = themeProvider;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWork, HttpGet, Route("api/Theme")]
		public virtual IHttpActionResult Index()
		{
			return Ok(new
			{
				Name = _themeProvider.GetByOwner(_loggedOnUser.CurrentUser())
			});
		}

		[UnitOfWork, HttpPost, Route("api/Theme")]
		public virtual void Change(string name)
		{
			_themeProvider.Persist(new ThemeSetting {Name = name});
		}
	}
}