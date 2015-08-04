using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
			var checkName = _tenantExists.Check(model.Tenant);
			if (!checkName.Success)
				return Json(new CreateTenantResultModel {Message = checkName.Message, Success = false});

			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				DataSource = model.Server,
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = "master",
				IntegratedSecurity = false
			};

			var checkServer = checkServerInternal(builder);
            if (!checkServer.Success)
				return Json(new CreateTenantResultModel { Message = checkServer.Message, Success = false });
			
			
			var dbPath = _dbPathProvider.GetDbPath();
			
			_databaseHelperWrapper.CreateLogin(builder.ConnectionString, model.AppUser, model.AppPassword);

			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmApp";
			
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString,DatabaseType.TeleoptiCCC7, dbPath, model.AppUser);
			var personId = Guid.NewGuid();
			_databaseHelperWrapper.AddInitialPerson(builder.ConnectionString, personId);
			_databaseHelperWrapper.AddBusinessUnit(builder.ConnectionString, model.BusinessUnit);
			builder.UserID = model.AppUser;
			builder.Password = model.AppPassword;
			var connstringApp = builder.ConnectionString;

			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmAnalytics";
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiAnalytics, dbPath, model.AppUser);
			builder.UserID = model.AppUser;
			builder.Password = model.AppPassword;
			var connstringAnalytics = builder.ConnectionString;

			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			var updateViewsConnstringAnalyticsUpdateViews = builder.ConnectionString;


			builder.UserID = model.CreateDbUser;
			builder.Password = model.CreateDbPassword;
			builder.InitialCatalog = model.Tenant + "_TeleoptiWfmAgg";
			_databaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCCAgg, dbPath, model.AppUser);

			var newTenant = new Tenant(model.Tenant);
			newTenant.SetApplicationConnectionString(connstringApp);
			newTenant.SetAnalyticsConnectionString(connstringAnalytics);
			_currentTenantSession.CurrentSession().Save(newTenant);

			var personInfo = new PersonInfo(newTenant, personId);
			
			// todo handle passwordStrength error this is just to get it to work, the loader is in applicationdata and I don't think it is so good
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.FirstUser, model.FirstUserPassword);
			_currentTenantSession.CurrentSession().Save(personInfo);

			UpdateCrossDatabaseView.Execute(updateViewsConnstringAnalyticsUpdateViews, model.Tenant + "_TeleoptiWfmAgg");

			//takes around 30 sek so we should present some feedback to the user meanwhile, signalr?
			return Json(new CreateTenantResultModel {Success = true, Message = "created databases"});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckServer")]
		public virtual JsonResult<CreateTenantResultModel> CheckServer(CreateTenantModel model)
		{
			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				DataSource = model.Server,
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = "master",
				IntegratedSecurity = false
			};
			var checkServer = checkServerInternal(builder);
			return Json(new CreateTenantResultModel { Message = checkServer.Message, Success = checkServer.Success });
		}

		private CreateTenantResultModel checkServerInternal( SqlConnectionStringBuilder builder)
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

			return new CreateTenantResultModel { Success = true, Message = "The user does have permission to create database." };
		}
    }

	public class CreateTenantModel
	{
		public string Tenant { get; set; }
		public string Server { get; set; }
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