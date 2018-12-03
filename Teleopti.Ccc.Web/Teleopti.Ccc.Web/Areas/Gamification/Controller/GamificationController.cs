using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.Gamification)]
	public class GamificationController : ApiController
	{
		private readonly string CSV_CONTENT_TYPE = "text/csv";
		private readonly IGamificationSettingPersister _gamificationSettingPersister;
		private readonly IGamificationSettingProvider _gamificationSettingProvider;
		private readonly ISiteViewModelFactory _siteViewModelFactory;
		private readonly ITeamGamificationSettingProviderAndPersister _gamificationSettingProviderAndPersister;
		private readonly IMultipartHttpContentExtractor _multipartHttpContentExtractor;
		private readonly IImportExternalPerformanceInfoService _importExternalPerformanceInfoService;

		public GamificationController(IGamificationSettingPersister gamificationSettingPersister, IGamificationSettingProvider gamificationSettingProvider, 
			ISiteViewModelFactory siteViewModelFactory, ITeamGamificationSettingProviderAndPersister gamificationSettingProviderAndPersister, IMultipartHttpContentExtractor multipartHttpContentExtractor, IImportExternalPerformanceInfoService importExternalPerformanceInfoService)
		{
			_gamificationSettingPersister = gamificationSettingPersister;
			_gamificationSettingProvider = gamificationSettingProvider;
			_siteViewModelFactory = siteViewModelFactory;
			_gamificationSettingProviderAndPersister = gamificationSettingProviderAndPersister;
			_multipartHttpContentExtractor = multipartHttpContentExtractor;
			_importExternalPerformanceInfoService = importExternalPerformanceInfoService;
		}

		[HttpPost, Route("api/Gamification/NewImportExternalPerformanceInfoJob")]
		public async Task<OkResult> NewImportExternalPerformanceInfoJob()
		{
			var contents = await ImportCommonAction.ReadAsMultipartAsync(Request.Content);
			var fileData = _multipartHttpContentExtractor.ExtractFileData(contents).SingleOrDefault();

			createJob(fileData);
			return Ok();
		}


		[Route("api/gamification/import-jobs"), HttpGet, UnitOfWork]
		public virtual object GetJobList()
		{
			return _importExternalPerformanceInfoService.GetJobsForCurrentBusinessUnit();
		}

		[UnitOfWork]
		protected virtual void createJob(ImportFileData fileData)
		{
			if (fileData?.Data?.Length == 0)
			{
				throw new ArgumentNullException(Resources.File, Resources.NoInput);
			}
			_importExternalPerformanceInfoService.CreateJob(fileData);
		}

		[HttpGet, Route("api/Gamification/job/{id}/artifact/{category}"), UnitOfWork]
		public virtual HttpResponseMessage DownloadArtifact(Guid id, JobResultArtifactCategory category)
		{
			var response = Request.CreateResponse();
			var artifact = _importExternalPerformanceInfoService.GetJobResultArtifact(id, category);
			if (artifact == null)
			{
				response.StatusCode = HttpStatusCode.NotFound;
				return response;
			}

			response.Content = new ByteArrayContent(artifact.Content);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(CSV_CONTENT_TYPE);
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = artifact.Name
			};
			return response;
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

		[HttpGet, Route("api/Gamification/Load/{id}"), UnitOfWork]
		public virtual GamificationSettingViewModel LoadGamification(Guid id)
		{
			return _gamificationSettingProvider.GetGamificationSetting(id);
		}

		[HttpGet, Route("api/Gamification/LoadGamificationList"), UnitOfWork]
		public virtual IList<GamificationDescriptionViewModel> LoadGamificationList()
		{
			return _gamificationSettingProvider.GetGamificationList();
		}

		[HttpPost, Route("api/Gamification/ModifyDescription"), UnitOfWork]
		public virtual GamificationDescriptionViewModel GamificationDescription([FromBody]GamificationDescriptionForm input)
		{
			return _gamificationSettingPersister.PersistDescription(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAnsweredCallsEnabled"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel  GamificationAnsweredCallsEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsEnabled(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAnsweredCallsForGold"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForGold([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAnsweredCallsForSilver"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForSilver([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAnsweredCallsForBronze"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCallsForBronze([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAHTEnabled"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel GamificationAHTEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTEnabled(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAHTForGold"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForGold([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAHTForSilver"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForSilver([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAHTForBronze"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTForBronze([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAdherenceEnabled"), UnitOfWork]
		public virtual GamificationThresholdEnabledViewModel GamificationAdherenceEnabled([FromBody]GamificationThresholdEnabledViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceEnabled(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAdherenceForGold"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForGold([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAdherenceForSilver"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForSilver([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAdherenceForBronze"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherenceForBronze([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyChangeRule"), UnitOfWork]
		public virtual GamificationSettingViewModel GamificationChangeRule([FromBody]GamificationChangeRuleForm input)
		{
			return _gamificationSettingPersister.PersistRuleChange(input);
		}

		[HttpPost, Route("api/Gamification/ModifyRollingPeriod"), UnitOfWork]
		public virtual GamificationSettingViewModel GamificationModifyRollingPeriod([FromBody]GamificationModifyRollingPeriodForm input)
		{
			return _gamificationSettingPersister.PersistRollingPeriodChange(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAnsweredCalls"), UnitOfWork]
		public virtual GamificationAnsweredCallsThresholdViewModel GamificationAnsweredCalls([FromBody]GamificationAnsweredCallsThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAnsweredCallsThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAHTThreshold"), UnitOfWork]
		public virtual GamificationAHTThresholdViewModel GamificationAHTThreshold([FromBody]GamificationAHTThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAHTThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyAdherence"), UnitOfWork]
		public virtual GamificationAdherenceThresholdViewModel GamificationAdherence([FromBody]GamificationAdherenceThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistAdherenceThreshold(input);
		}

		[HttpPost, Route("api/Gamification/ModifyGoldToSilverRate"), UnitOfWork]
		public virtual GamificationBadgeConversRateViewModel GamificationGoldToSilverRate([FromBody]GamificationBadgeConversRateViewModel input)
		{
			return _gamificationSettingPersister.PersistGoldToSilverRate(input);
		}

		[HttpPost, Route("api/Gamification/ModifySilverToBronzeRate"), UnitOfWork]
		public virtual GamificationBadgeConversRateViewModel GamificationSilverToBronzeRate([FromBody]GamificationBadgeConversRateViewModel input)
		{
			return _gamificationSettingPersister.PersistSilverToBronzeRate(input);
		}

		[HttpPost, Route("api/Gamification/LoadSites"), UnitOfWork]
		public virtual IEnumerable<SelectOptionItem> GetAllSites()
		{
			return _siteViewModelFactory.CreateSiteOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.Gamification);
		}

		[HttpPost, Route("api/Gamification/LoadTeamGamification"), UnitOfWork]
		public virtual IEnumerable<TeamGamificationSettingViewModel> GetTeamGamificationSettings(List<Guid> siteIds)
		{
			return _gamificationSettingProviderAndPersister.GetTeamGamificationSettingViewModels(siteIds);
		}

		[HttpPost, Route("api/Gamification/SetTeamGamification"), UnitOfWork]
		public virtual IEnumerable<TeamGamificationSettingViewModel> SetTeamGamificationSetting(TeamsGamificationSettingForm input)
		{
			return _gamificationSettingProviderAndPersister.SetTeamsGamificationSetting(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingDescription"), UnitOfWork]
		public virtual ExternalBadgeSettingDescriptionViewModel UpdateExternalBadgeSettingDescription([FromBody] ExternalBadgeSettingDescriptionViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeDescription(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingThreshold"), UnitOfWork]
		public virtual ExternalBadgeSettingThresholdViewModel UpdateExternalBadgeSettingThreshold([FromBody] ExternalBadgeSettingThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingGoldThreshold"), UnitOfWork]
		public virtual ExternalBadgeSettingThresholdViewModel UpdateExternalBadgeSettingGoldThreshold([FromBody] ExternalBadgeSettingThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeGoldThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingSilverThreshold"), UnitOfWork]
		public virtual ExternalBadgeSettingThresholdViewModel UpdateExternalBadgeSettingSilverThreshold([FromBody] ExternalBadgeSettingThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeSilverThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingBronzeThreshold"), UnitOfWork]
		public virtual ExternalBadgeSettingThresholdViewModel UpdateExternalBadgeSettingBronzeThreshold([FromBody] ExternalBadgeSettingThresholdViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeBronzeThreshold(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingEnabled"), UnitOfWork]
		public virtual ExternalBadgeSettingBooleanViewModel UpdateExternalBadgeSettingEnabled([FromBody] ExternalBadgeSettingBooleanViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeEnabled(input);
		}

		[HttpPost, Route("api/Gamification/Update/ExternalBadgeSettingLargerIsBetter"), UnitOfWork]
		public virtual ExternalBadgeSettingBooleanViewModel UpdateExternalBadgeSettingLargerIsBetter([FromBody] ExternalBadgeSettingBooleanViewModel input)
		{
			return _gamificationSettingPersister.PersistExternalBadgeLargerIsBetter(input);
		}
	}
}