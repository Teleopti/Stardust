using System;
using System.Web.Http;
using Microsoft.ServiceBus;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using WebGrease.Css.ImageAssemblyAnalysis.LogModel;

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

		[HttpDelete, Route("api/Gamification/Delete/{Id}"), UnitOfWork]
		public virtual IHttpActionResult RemoveGamification(Guid Id)
		{
			var isSuccess = _gamificationSettingPersister.RemoveGamificationSetting(Id);

			if (isSuccess) return Ok();
			return NotFound();
		}

		[HttpPost, Route("api/Gamification/Load"), UnitOfWork]
		public virtual GamificationSettingViewModel LoadGamification(Guid id)
		{
			return _gamificationSettingProvider.GetGamificationSetting(id);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationDescriptionViewMode GamificationDescription([FromBody]GamificationDescriptionViewMode input)
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
		public virtual GamificationAdherenceThresholdViewModel GamificationAHTForSilver([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Modify"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAHTForBronze([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Reset"), UnitOfWork]
		public virtual bool ResetGamification()
		{
			return _gamificationSettingPersister.ResetGamificationSetting();
		}
	}
}