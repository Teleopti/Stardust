using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Security;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class DatabaseController : ApiController
	{
		private readonly DatabaseHelperWrapper _databaseHelperWrapper;
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ITenantExists _tenantExists;
		private readonly IDbPathProvider _dbPathProvider;
		//private readonly ICheckPasswordStrength _checkPasswordStrength;

		public DatabaseController(DatabaseHelperWrapper databaseHelperWrapper,  ICurrentTenantSession currentTenantSession, ITenantExists tenantExists, IDbPathProvider dbPathProvider)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_currentTenantSession = currentTenantSession;
			_tenantExists = tenantExists;
			_dbPathProvider = dbPathProvider;
			//_checkPasswordStrength = checkPasswordStrength;
		}


		[HttpPost]
		[TenantUnitOfWork]
		[Route("CreateTenant")]
		public virtual JsonResult<CreateTenantResultModel> CreateDatabases(CreateTenantModel model)
		{
			//actually if the login already exists we could skip the password
			if(string.IsNullOrEmpty(model.AppUser) || string.IsNullOrEmpty(model.AppPassword))
				return Json(new CreateTenantResultModel { Message = "Both name and password for the login must be filled in.", Success = false });

			var checkFirstuser = checkFirstUserInternal(model.FirstUser, model.FirstUserPassword);
			if(!checkFirstuser.Success)
				return Json(new CreateTenantResultModel { Message = checkFirstuser.Message, Success = false });

			if(string.IsNullOrEmpty(model.BusinessUnit))
				return Json(new CreateTenantResultModel { Message = "The Business Unit can not be empty.", Success = false });

			var checkName = _tenantExists.Check(model.Tenant);
			if (!checkName.Success)
				return Json(new CreateTenantResultModel {Message = checkName.Message, Success = false});

			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = "master",
				IntegratedSecurity = false
			};

			var checkCreate = checkCreateDbInternal(builder);
            if (!checkCreate.Success)
				return Json(new CreateTenantResultModel { Message = checkCreate.Message, Success = false });

			var isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

         var dbPath = _dbPathProvider.GetDbPath();
			
			_databaseHelperWrapper.CreateLogin(builder.ConnectionString, model.AppUser, model.AppPassword);

			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmApp";
			
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString,DatabaseType.TeleoptiCCC7, dbPath, model.AppUser, isAzure);
			var personId = Guid.NewGuid();
			_databaseHelperWrapper.AddInitialPerson(builder.ConnectionString, personId);
			_databaseHelperWrapper.AddBusinessUnit(builder.ConnectionString, model.BusinessUnit);
			builder.UserID = model.AppUser;
			builder.Password = model.AppPassword;
			var connstringApp = builder.ConnectionString;

			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmAnalytics";
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiAnalytics, dbPath, model.AppUser, isAzure);
			builder.UserID = model.AppUser;
			builder.Password = model.AppPassword;
			var connstringAnalytics = builder.ConnectionString;

			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			var updateViewsConnstringAnalyticsUpdateViews = builder.ConnectionString;


			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmAgg";
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCCAgg, dbPath, model.AppUser, isAzure);

			var newTenant = new Tenant(model.Tenant);
			newTenant.SetApplicationConnectionString(connstringApp);
			newTenant.SetAnalyticsConnectionString(connstringAnalytics);
			_currentTenantSession.CurrentSession().Save(newTenant);

			var personInfo = new PersonInfo(newTenant, personId);
			
			// todo handle passwordStrength error this is just to get it to work, the loader is in applicationdata and I don't think it is so good
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.FirstUser, model.FirstUserPassword);
			_currentTenantSession.CurrentSession().Save(personInfo);

			if(isAzure)
				UpdateCrossDatabaseView.Execute(updateViewsConnstringAnalyticsUpdateViews, model.Tenant + "_TeleoptiWfmAnalytics");
			else
				UpdateCrossDatabaseView.Execute(updateViewsConnstringAnalyticsUpdateViews, model.Tenant + "_TeleoptiWfmAgg");

			return Json(new CreateTenantResultModel {Success = true, Message = "Successfully created a new Tenant."});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckCreateDb")]
		public virtual JsonResult<CreateTenantResultModel> CheckCreateDb(CreateTenantModel model)
		{
			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = "master",
				IntegratedSecurity = false
			};
			return Json(checkCreateDbInternal(builder));
		}

		private CreateTenantResultModel checkCreateDbInternal( SqlConnectionStringBuilder builder)
		{
			var connection = new SqlConnection(builder.ConnectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new CreateTenantResultModel { Success = false, Message = "Can not connect to the database. " + e.Message };
			}

			if (!_databaseHelperWrapper.HasCreateDbPermission(builder.ConnectionString))
				return new CreateTenantResultModel { Success = false, Message = "The user does not have permission to create database." };

			if (!_databaseHelperWrapper.HasCreateDbPermission(builder.ConnectionString))
				return new CreateTenantResultModel { Success = false, Message = "The user does not have permission to create views." };

			return new CreateTenantResultModel { Success = true, Message = "The user does have permission to create database and views." };
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckLogin")]
		public virtual JsonResult<CreateTenantResultModel> CheckLogin(CreateTenantModel model)
		{
			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = "master",
				IntegratedSecurity = false
			};
			if (_databaseHelperWrapper.LoginExists(builder.ConnectionString, model.AppUser))
				return
					Json(new CreateTenantResultModel
					{
						Success = true,
						Message = "The login already exists, the password will NOT be changed!"
					});

			return
					Json(new CreateTenantResultModel
					{
						Success = true,
						Message = "The login does not exists, it will be created."
					});

		}


		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckFirstUser")]
		public virtual JsonResult<CreateTenantResultModel> CheckFirstUser(CreateTenantModel model)
		{
			return Json(checkFirstUserInternal(model.FirstUser, model.FirstUserPassword));
		}

		private CreateTenantResultModel checkFirstUserInternal(string name, string password)
		{

			if (string.IsNullOrEmpty(name))
				return new CreateTenantResultModel { Success = false, Message = "The user name can not be empty." };
			if (string.IsNullOrEmpty(password))
				return new CreateTenantResultModel { Success = false, Message = "The password can not be empty." };

			var mainUsers = _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();
			var exists = mainUsers.FirstOrDefault(m => m.ApplicationLogonInfo.LogonName.Equals(name,StringComparison.InvariantCultureIgnoreCase));
			if (exists != null)
				return new CreateTenantResultModel { Success = false, Message = "The user already exists." };

			return new CreateTenantResultModel { Success = true, Message = "The user name is ok." };
		}

		private CreateTenantResultModel checkAppUserInternal(string name, string password)
		{

			if (string.IsNullOrEmpty(name))
				return new CreateTenantResultModel { Success = false, Message = "The login can not be empty." };
			if (string.IsNullOrEmpty(password))
				return new CreateTenantResultModel { Success = false, Message = "The login password can not be empty." };

			

			return new CreateTenantResultModel { Success = true, Message = "The user name is ok." };
		}
	}

	public class CreateTenantModel
	{
		public string Tenant { get; set; }

		public string CreateDbUser { get; set; }
		public string CreateDbPassword { get; set; }
		public string AppUser { get; set; }
		public string AppPassword { get; set; }

		public string FirstUser { get; set; }
		public string FirstUserPassword { get; set; }

		public string BusinessUnit { get; set; }
	}

	public class CreateTenantResultModel
	{
		public string Message { get; set; }
		public bool Success { get; set; }

	}
}