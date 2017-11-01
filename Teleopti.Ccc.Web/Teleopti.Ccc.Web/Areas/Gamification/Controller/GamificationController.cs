using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	public class GamificationController : ApiController
	{
		private readonly IGamificationSettingPersister _gamificationSettingPersister;
		private readonly IGamificationSettingProvider _gamificationSettingProvider;

		public GamificationController(IGamificationSettingPersister gamificationSettingPersister, IGamificationSettingProvider gamificationSettingProvider)
		{
			_gamificationSettingPersister = gamificationSettingPersister;
			_gamificationSettingProvider = gamificationSettingProvider;
		}

		[HttpPost, Route("api/Gamification/Create"), UnitOfWork]
		public virtual GamificationSettingViewModel CreateGamification()
		{
			return _gamificationSettingPersister.Persist();
		}

		[HttpPost, Route("api/Gamification/Load"), UnitOfWork]
		public virtual GamificationSettingViewModel LoadGamification(Guid id)
		{
			return _gamificationSettingProvider.GetGamificationSetting(id);
		}
	}
}