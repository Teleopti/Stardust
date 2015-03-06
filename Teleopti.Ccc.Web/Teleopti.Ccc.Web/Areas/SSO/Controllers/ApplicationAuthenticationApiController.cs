using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		private readonly IApplicationData _applicationData;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;

		public ApplicationAuthenticationApiController(IApplicationData applicationData, 
																							IRepositoryFactory repositoryFactory, 
																							ILoadPasswordPolicyService loadPasswordPolicyService, 
																							ICurrentPrincipalContext currentPrincipalContext, 
																							IFormsAuthentication formsAuthentication,
																							IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_applicationData = applicationData;
			_currentPrincipalContext = currentPrincipalContext;
			_formsAuthentication = formsAuthentication;
			_applicationUserTenantQuery = applicationUserTenantQuery;
			_repositoryFactory = repositoryFactory;
			_loadPasswordPolicyService = loadPasswordPolicyService;
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult CheckPassword(ApplicationAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
			{
				if (result.PasswordExpired)
				{
					return Json(new PasswordWarningViewModel {AlreadyExpired = true}, JsonRequestBehavior.AllowGet);
				}
				model.SaveAuthenticateResult(result);
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", result.Message);
				return ModelState.ToJson();
			}

			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier);
			model.SaveAuthenticateResult(result);
			return Json(new PasswordWarningViewModel { WillExpireSoon = result.HasMessage}, JsonRequestBehavior.AllowGet);
		}

		[HttpPostOrPut]
		public JsonResult ChangePassword(ChangePasswordInput model)
		{
			var personInfo = _applicationUserTenantQuery.Find(model.UserName);
			if (personInfo == null)
				throw new HttpException(500, "person not found");

			var dataSource = _applicationData.DataSource(personInfo.Tenant);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.LoadOne(personInfo.Id);
				_currentPrincipalContext.SetCurrentPrincipal(person, dataSource, null);
				var userDetailRepository = _repositoryFactory.CreateUserDetailRepository(uow);
				var result = person.ChangePassword(model.OldPassword, model.NewPassword, _loadPasswordPolicyService, userDetailRepository.FindByUser(person));
				uow.PersistAll();
				if (!result.IsSuccessful)
				{
					Response.StatusCode = 400;
					Response.TrySkipIisCustomErrors = true;
					ModelState.AddModelError("Error", result.IsAuthenticationSuccessful ? Resources.PasswordPolicyWarning : Resources.InvalidUserNameOrPassword);
					return ModelState.ToJson();
				}
				_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier);
				return Json(result);
			}
		}

		
	}
}