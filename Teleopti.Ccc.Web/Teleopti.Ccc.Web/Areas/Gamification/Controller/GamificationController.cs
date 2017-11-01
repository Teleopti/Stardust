using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Gamification.core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	public class GamificationController : ApiController
	{
		private readonly IGamificationSettingPersister _gamificationSettingPersister;

		public GamificationController(IGamificationSettingPersister gamificationSettingPersister)
		{
			_gamificationSettingPersister = gamificationSettingPersister;
		}

		[HttpPost, Route("api/Gamification/Create"), UnitOfWork]
		public virtual GamificationViewModel CreateGamification()
		{
			return _gamificationSettingPersister.Persist();
		}
	}
}