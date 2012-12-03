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

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;

		public ApplicationAuthenticationApiController(IDataSourcesProvider dataSourceProvider, IRepositoryFactory repositoryFactory, ILoadPasswordPolicyService loadPasswordPolicyService, ICurrentPrincipalContext currentPrincipalContext)
		{
			_currentPrincipalContext = currentPrincipalContext;
			_dataSourceProvider = dataSourceProvider;
			_repositoryFactory = repositoryFactory;
			_loadPasswordPolicyService = loadPasswordPolicyService;
		}

		[HttpGet]
		public JsonResult CheckPassword(IAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
			{
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", result.Message);
				if (result.PasswordExpired)
					Response.SubStatusCode = 1;
				return ModelState.ToJson();
			}
			var passwordWarningViewModel = new PasswordWarningViewModel();
			if (result.Successful && result.HasMessage)
			{
				passwordWarningViewModel.Warning = result.Message;
				passwordWarningViewModel.WillExpireSoon = true;
			}else
			{
				passwordWarningViewModel.WillExpireSoon = false;
			}
			return Json(passwordWarningViewModel, JsonRequestBehavior.AllowGet);
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
				if (!result.IsSuccessful)
				{
					Response.StatusCode = 400;
					Response.TrySkipIisCustomErrors = true;
					ModelState.AddModelError("Error", Resources.InvalidUserNameOrPassword);
					return ModelState.ToJson();
				}
				uow.PersistAll();
				return Json(result);
			}
		}
	}

	public class PasswordWarningViewModel
	{
		public bool WillExpireSoon { get; set; }
		public string Warning { get; set; }
	}

	public class ChangePasswordInput
	{
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
		public string UserName { get; set; }
		public string DataSourceName { get; set; }
	}
}