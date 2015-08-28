using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class UpgradeDatabasesController : ApiController
	{
		private readonly ICheckDatabaseVersions _checkDatabaseVersions;

		public UpgradeDatabasesController(ICheckDatabaseVersions checkDatabaseVersions)
		{
			_checkDatabaseVersions = checkDatabaseVersions;
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/UpgradeDatabases/GetVersions")]
		public virtual JsonResult<VersionResultModel> GetVersions(VersionCheckModel model)
		{
			if (string.IsNullOrEmpty(model.Server) || string.IsNullOrEmpty(model.AppDatabase) || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new VersionResultModel {   AppVersionOk= false, Error = "All properties must be filled in." });

			var appBuilder = new SqlConnectionStringBuilder { DataSource = model.Server, InitialCatalog = model.AppDatabase, UserID = model.UserName, Password = model.Password };
			return Json(_checkDatabaseVersions.GetVersions(appBuilder.ConnectionString));

		}
	}
}
