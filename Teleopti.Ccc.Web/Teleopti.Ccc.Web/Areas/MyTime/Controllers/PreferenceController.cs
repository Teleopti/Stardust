using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[PreferencePermission]
	public class PreferenceController : Controller
	{
		private readonly IPreferenceViewModelFactory _viewModelFactory;
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;
		private readonly IPreferencePersister _preferencePersister;
		private readonly IPreferenceTemplatePersister _preferenceTemplatePersister;

		public PreferenceController(IPreferenceViewModelFactory viewModelFactory, IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider, IPreferencePersister preferencePersister, IPreferenceTemplatePersister preferenceTemplatePersister)
		{
			_viewModelFactory = viewModelFactory;
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
			_preferencePersister = preferencePersister;
			_preferenceTemplatePersister = preferenceTemplatePersister;
		}

		[EnsureInPortal]
		[UnitOfWork]
		public virtual ViewResult Index(DateOnly? date)
		{
			date = !date.HasValue ? _virtualSchedulePeriodProvider.CalculatePreferenceDefaultDate() : _virtualSchedulePeriodProvider.GetCurrentOrNextVirtualPeriodForDate(date.Value).StartDate;

			if (_virtualSchedulePeriodProvider.MissingPersonPeriod(date))
				return View("NoPersonPeriodPartial");
			if (_virtualSchedulePeriodProvider.MissingSchedulePeriod())
				return View("NoSchedulePeriodPartial");
			
			return View("PreferencePartial", _viewModelFactory.CreateViewModel(date.Value));
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult PreferencesAndSchedules(DateOnly @from, DateOnly to)
		{
			return Json(_viewModelFactory.CreatePreferencesAndSchedulesViewModel(from, to), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[UnitOfWork]
		public virtual JsonResult WeeklyWorkTimeSetting(DateOnly date)
		{
			return Json(_viewModelFactory.CreatePreferenceWeeklyWorkTimeViewModel(date), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		[ActionName("Preference")]
		public virtual JsonResult GetPreference(DateOnly date)
		{
			var model = _viewModelFactory.CreateDayViewModel(date);
			if (model==null)
			{
				Response.StatusCode = (int) HttpStatusCode.NoContent;
				return null;
			}
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult Preference(PreferenceDayInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_preferencePersister.Persist(input));
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult MustHave(MustHaveInput input)
		{
			return Json(_preferencePersister.MustHave(input));
		}

		
		[UnitOfWork]
		[HttpPost]
		public virtual JsonResult PreferenceDelete(DateOnly[] dateList)
		{
			return Json(_preferencePersister.Delete(dateList.ToList()), JsonRequestBehavior.AllowGet);
		}
		
		[HttpGet]
		[UnitOfWork]
		public virtual  JsonResult GetPreferenceTemplates()
		{
			return Json(_viewModelFactory.CreatePreferenceTemplateViewModels(), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult PreferenceTemplate(PreferenceTemplateInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_preferenceTemplatePersister.Persist(input));
		}

		[UnitOfWork]
		[HttpDelete]
		[ActionName("PreferenceTemplate")]
		public virtual JsonResult PreferenceTemplateDelete(Guid id)
		{
			_preferenceTemplatePersister.Delete(id);
			return Json("");
		}

		[UnitOfWork]
		[HttpGet]
		[ActionName("ValidatePreference")]	//this validates preference (eg.: shift category preference, day off preference), not the preference template created by agent
		public virtual JsonResult CheckPreferenceValidation(DateOnly date, Guid? preferenceId)
		{
			const JsonRequestBehavior requestOption = JsonRequestBehavior.AllowGet;

			var preferenceModel = _viewModelFactory.CreateViewModel(date);
			if (preferenceModel == null || preferenceModel.Options == null)
			{
				return Json(new
				{
					isValid = false,
					message = Resources.CannotGetPreferenceSetting
				}, requestOption);
			}

			if (preferenceId != null)
			{
				var preferenceExisted =
					preferenceModel.Options.PreferenceOptions.Any(x => x.Options.Any(y => new Guid(y.Value) == preferenceId));

				return Json(new
				{
					isValid = preferenceExisted,
					message = preferenceExisted ? string.Empty : Resources.CannotAddPreferenceSelectedItemNotAvailable
				}, requestOption);
			}
			//when user creates or loads preference template without choosing any shift category preference , then preferenceID will be null, 
			//in this case, nothing to validate, just return empty validation message
			return Json(new {isValid = true, message = string.Empty}, requestOption);
		}
	}
}