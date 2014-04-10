using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;

		public ApplicationAuthenticationApiController(IDataSourcesProvider dataSourceProvider, IRepositoryFactory repositoryFactory, ILoadPasswordPolicyService loadPasswordPolicyService, ICurrentPrincipalContext currentPrincipalContext, IFormsAuthentication formsAuthentication)
		{
			_currentPrincipalContext = currentPrincipalContext;
			_formsAuthentication = formsAuthentication;
			_dataSourceProvider = dataSourceProvider;
			_repositoryFactory = repositoryFactory;
			_loadPasswordPolicyService = loadPasswordPolicyService;
		}

		[HttpGet]
		public JsonResult CheckPassword(ApplicationAuthenticationModel model)
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

			_formsAuthentication.SetAuthCookie(model.UserName + "§" + model.DataSourceName);
			model.SaveAuthenticateResult(result);
			return Json(new PasswordWarningViewModel { WillExpireSoon = result.HasMessage}, JsonRequestBehavior.AllowGet);
		}

		[HttpPostOrPut]
		public JsonResult ChangePassword(ChangePasswordInput model)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(model.DataSourceName);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.TryFindBasicAuthenticatedPerson(model.UserName);
				if (person == null)
				{
					throw new HttpException(500, "person not found");
				}
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
				_formsAuthentication.SetAuthCookie(model.UserName + "§" + model.DataSourceName);
				return Json(result);
			}
		}

		
	}
}