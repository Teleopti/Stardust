using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	public class GamificationController : ApiController
	{
		private readonly IGamificationSettingPersister _gamificationSettingPersister;
		private readonly IGamificationSettingProvider _gamificationSettingProvider;

		public GamificationController(IGamificationSettingPersister gamificationSettingPersister,
			IGamificationSettingProvider gamificationSettingProvider)
		{
			_gamificationSettingPersister = gamificationSettingPersister;
			_gamificationSettingProvider = gamificationSettingProvider;
		}

		[HttpPost, Route("api/Gamification/Create"), UnitOfWork]
		public virtual GamificationSettingViewModel CreateGamification()
		{
			return _gamificationSettingPersister.Persist();
		}

		[HttpDelete, Route("api/Gamification/Delete/{Id}"), UnitOfWork]
		public virtual IHttpActionResult RemoveGamification(Guid id)
		{
			var isSuccess = _gamificationSettingPersister.RemoveGamificationSetting(id);

			if (isSuccess) return Ok();
			return NotFound();
		}

		[HttpPost, Route("api/Gamification/Load"), UnitOfWork]
		public virtual GamificationSettingViewModel LoadGamification(Guid id)
		{
			return _gamificationSettingProvider.GetGamificationSetting(id);
		}

		[HttpGet, Route("api/Gamification/Load"), UnitOfWork]
		public virtual IList<GamificationDescriptionViewModel> LoadGamificationList()
		{
			return _gamificationSettingProvider.GetGamificationList();
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationDescriptionViewModel GamificationDescription([FromBody]GamificationDescriptionViewModel input)
		{
			return _gamificationSettingPersister.PersistDescription(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel  GamificationAnsweredCallsEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsEnabled(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForGold([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForSilver([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForBronze([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel GamificationAHTEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTEnabled(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForGold([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForSilver([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForBronze([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel GamificationAdherenceEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceEnabled(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForGold([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForSilver([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForBronze([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Reset"), UnitOfWork]
		public virtual bool ResetGamification()
		{
			return _gamificationSettingPersister.ResetGamificationSetting();
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationSettingViewModel GamificationChangeRule([FromBody]GamificationChangeRuleForm input)
		{
			return _gamificationSettingPersister.PersistRuleChange(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCalls([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTThreshold([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceFor([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationBadgeConversRateViewModel GamificationGoldToSilverRate([FromBody]GamificationBadgeConversRateViewModel input)
		{
			return _gamificationSettingPersister.PersistGoldToSilverRate(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationBadgeConversRateViewModel GamificationSilverToBronzeRate([FromBody]GamificationBadgeConversRateViewModel input)
		{
			return _gamificationSettingPersister.PersistSilverToBronzeRate(input);
		}
	}
}