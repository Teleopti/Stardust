﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class DatabaseController : ApiController
	{
		private readonly IDatabaseHelperWrapper _databaseHelperWrapper;
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ITenantExists _tenantExists;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ICheckPasswordStrength _checkPasswordStrength;
		private readonly PersistTenant _persistTenant;
		private readonly IUpdateCrossDatabaseView _updateCrossDatabaseView;
		private readonly ICreateBusinessUnit _createBusinessUnit;
		private readonly IHashFunction _currentHashFunction;

		public DatabaseController(
			IDatabaseHelperWrapper databaseHelperWrapper, 
			ICurrentTenantSession currentTenantSession,
			ITenantExists tenantExists, 
			ILoadAllTenants loadAllTenants,
			ICheckPasswordStrength checkPasswordStrength,
			PersistTenant persistTenant,
			IUpdateCrossDatabaseView updateCrossDatabaseView,
			ICreateBusinessUnit createBusinessUnit, IHashFunction currentHashFunction)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
			_currentTenantSession = currentTenantSession;
			_tenantExists = tenantExists;
			_loadAllTenants = loadAllTenants;
			_checkPasswordStrength = checkPasswordStrength;
			_persistTenant = persistTenant;
			_updateCrossDatabaseView = updateCrossDatabaseView;
			_createBusinessUnit = createBusinessUnit;
			_currentHashFunction = currentHashFunction;
		}


		[HttpPost]
		[TenantUnitOfWork]
		[Route("CreateTenant")]
		public virtual JsonResult<TenantResultModel> CreateDatabases(CreateTenantModel model)
		{
			var result = checkLoginInternal(model);
			if (!result.Success)
				return Json(new TenantResultModel { Message = result.Message, Success = false });

			var checkFirstuser = checkFirstUserInternal(model.FirstUser, model.FirstUserPassword);
			if (!checkFirstuser.Success)
				return Json(new TenantResultModel { Message = checkFirstuser.Message, Success = false });

			if (string.IsNullOrEmpty(model.BusinessUnit))
				return Json(new TenantResultModel { Message = "The Business Unit can not be empty.", Success = false });

			var checkName = _tenantExists.Check(model.Tenant);
			if (!checkName.Success)
				return Json(new TenantResultModel { Message = checkName.Message, Success = false });

			var connectionToNewDb = createLoginConnectionString(model);
			var appDbConnectionString = createAppDbConnectionString(model);
			var analyticsDbConnectionString = createAnalyticsDbConnectionString(model);
			var checkCreate = checkCreateDbInternal(connectionToNewDb);
			if (!checkCreate.Success)
				return Json(new TenantResultModel { Message = checkCreate.Message, Success = false });

			var newTenant = new Tenant(model.Tenant);
			newTenant.DataSourceConfiguration.SetApplicationConnectionString(appConnectionString(model));
			newTenant.DataSourceConfiguration.SetAnalyticsConnectionString(analyticsConnectionString(model));
			newTenant.DataSourceConfiguration.SetAggregationConnectionString(aggConnectionString(model));
			_persistTenant.Persist(newTenant);

			var version = _databaseHelperWrapper.Version(connectionToNewDb);
			_databaseHelperWrapper.CreateLogin(connectionToNewDb, model.AppUser, model.AppPassword, version);
			_databaseHelperWrapper.CreateDatabase(appDbConnectionString, DatabaseType.TeleoptiCCC7, model.AppUser, model.AppPassword, version, model.Tenant, newTenant.Id);
			
			_databaseHelperWrapper.CreateDatabase(analyticsDbConnectionString, DatabaseType.TeleoptiAnalytics, model.AppUser, model.AppPassword, version, model.Tenant, newTenant.Id);
			_databaseHelperWrapper.CreateDatabase(createAggDbConnectionString(model), DatabaseType.TeleoptiCCCAgg, model.AppUser, model.AppPassword, version, model.Tenant, newTenant.Id);

			_createBusinessUnit.Create(newTenant, model.BusinessUnit);

			_updateCrossDatabaseView.Execute(analyticsDbConnectionString,
				version.IsAzure ? $"{model.Tenant}_TeleoptiWfmAnalytics" : $"{model.Tenant}_TeleoptiWfmAgg");

			addSystemUserToTenant(newTenant, "first", "user", model.FirstUser, model.FirstUserPassword);

			return Json(new TenantResultModel { Success = true, Message = "Successfully created a new Tenant.", TenantId = newTenant.Id });
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckCreateDb")]
		public virtual JsonResult<TenantResultModel> CheckCreateDb(CreateTenantModel model)
		{
			return Json(checkCreateDbInternal(createLoginConnectionString(model)));
		}

		private static string createLoginConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = DatabaseHelper.MasterDatabaseName,
				IntegratedSecurity = false
			}.ConnectionString;
		}

		private static string createAppDbConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = $"{model.Tenant}_TeleoptiWfmApp",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private static string createAnalyticsDbConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = $"{model.Tenant}_TeleoptiWfmAnalytics",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private static string createAggDbConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword,
				InitialCatalog = model.Tenant + "_TeleoptiWfmAgg",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private static string appConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.AppUser,
				Password = model.AppPassword,
				InitialCatalog = $"{model.Tenant}_TeleoptiWfmApp",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private static string analyticsConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.AppUser,
				Password = model.AppPassword,
				InitialCatalog = $"{model.Tenant}_TeleoptiWfmAnalytics",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private static string aggConnectionString(CreateTenantModel model)
		{
			return new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = model.AppUser,
				Password = model.AppPassword,
				InitialCatalog = $"{model.Tenant}_TeleoptiWfmAgg",
				IntegratedSecurity = false,
			}.ConnectionString;
		}

		private TenantResultModel checkCreateDbInternal(string connectionString)
		{
			var connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new TenantResultModel { Success = false, Message = $"Can not connect to the database. {e.Message}"};
			}

			var version = _databaseHelperWrapper.Version(connectionString);
			if (!_databaseHelperWrapper.HasCreateDbPermission(connectionString, version))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create databases." };

			if (!_databaseHelperWrapper.HasCreateViewAndLoginPermission(connectionString, version))
				return new TenantResultModel { Success = false, Message = "The user does not have permission to create logins and views." };

			return new TenantResultModel { Success = true, Message = "The user does have permission to create databases, logins and views." };
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckLogin")]
		public virtual JsonResult<TenantResultModel> CheckLogin(CreateTenantModel model)
		{
			return Json(checkLoginInternal(model));
		}

		private TenantResultModel checkLoginInternal(CreateTenantModel model)
		{
			if (string.IsNullOrEmpty(model.AppUser) || string.IsNullOrEmpty(model.AppPassword))
				return new TenantResultModel { Message = "Both name and password for the login must be filled in.", Success = false };

			var regex = new Regex(@"^(?!.{31})(?=.{8})(?=.*[^A-Za-z])(?=.*[A-Z])(?=.*[a-z]).*$");
			if (!regex.IsMatch(model.AppPassword))
			{
				return new TenantResultModel
				{
					Success = false,
					Message = "Make sure you have entered a strong Password. Between 8 and 31 characters, at least one uppercase, one lowercase and one digit"
				};
			}

			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				IntegratedSecurity = false,
				UserID = model.CreateDbUser,
				Password = model.CreateDbPassword
			};
			try
			{
				var version = _databaseHelperWrapper.Version(builder.ConnectionString);

				if (_databaseHelperWrapper.LoginExists(builder.ConnectionString, model.AppUser, version))
				{
					return
					new TenantResultModel
					{
						Success = false,
						Message = "The login already exists you must create a new one."
					};
				}
				string message;
				if (!_databaseHelperWrapper.LoginCanBeCreated(builder.ConnectionString, model.AppUser, model.AppPassword, version, out message))
				{
					return
					new TenantResultModel
					{
						Success = false,
						Message = $"Login can not be created. {message}"
					};
				}

				return
						new TenantResultModel
						{
							Success = true,
							Message = "The login does not exists, it will be created."
						};
			}
			catch (Exception exception)
			{
				return
					new TenantResultModel
					{
						Success = false,
						Message = $"Login can not be created. {exception.Message}"
					};
			}
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckFirstUser")]
		public virtual JsonResult<TenantResultModel> CheckFirstUser(CreateTenantModel model)
		{
			return Json(checkFirstUserInternal(model.FirstUser, model.FirstUserPassword));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("AddSuperUserToTenant")]
		public virtual JsonResult<TenantResultModel> AddSystemUserToTenant(AddSuperUserToTenantModel model)
		{
			if (string.IsNullOrEmpty(model.Tenant))
			{
				return Json(new TenantResultModel { Success = false, Message = "The Tenant name can not be empty." });
			}
			var checkUser = checkFirstUserInternal(model.UserName, model.Password);
			if (!checkUser.Success)
				return Json(checkUser);

			var tenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(model.Tenant));
			if (tenant == null)
			{
				return Json(new TenantResultModel {Success = false, Message = "Can not find this Tenant in the database."});
			}
         return Json(addSystemUserToTenant(tenant,model.FirstName, model.LastName, model.UserName, model.Password));
		}
		
		[HttpPost]
		[TenantUnitOfWork]
		[Route("AddBusinessUnitToTenant")]
		public virtual JsonResult<TenantResultModel> AddBusinessUnitToTenant(AddBuToTenantModel model)
		{

			if (string.IsNullOrEmpty(model.BuName))
			{
				return Json(new TenantResultModel { Success = false, Message = "The Business Unit name can not be empty." });
			}
			if (string.IsNullOrEmpty(model.Tenant))
			{
				return Json(new TenantResultModel { Success = false, Message = "The Tenant name can not be empty." });
			}
			
			var tenant = _loadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals(model.Tenant));
			if (tenant == null)
			{
				return Json(new TenantResultModel { Success = false, Message = "Can not find this Tenant in the database." });
			}
			_createBusinessUnit.Create(tenant, model.BuName);

			return Json(new TenantResultModel {Message = $"Created new Business Unit wih name: {model.BuName}", Success = true});
		}
		private TenantResultModel checkFirstUserInternal(string name, string password)
		{

			if (string.IsNullOrEmpty(name))
				return new TenantResultModel { Success = false, Message = "The user name can not be empty." };
			if (string.IsNullOrEmpty(password))
				return new TenantResultModel { Success = false, Message = "The password can not be empty." };

			var mainUsers = _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();

			mainUsers = mainUsers.Where(m => m.ApplicationLogonInfo.LogonName != null).ToList();

			var exists = mainUsers.FirstOrDefault(m => m.ApplicationLogonInfo.LogonName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			if (exists != null)
				return new TenantResultModel { Success = false, Message = "The user already exists." };
			try
			{
				_checkPasswordStrength.Validate(password);
			}
			catch (Exception)
			{
				return new TenantResultModel { Success = false, Message = "The password does not meet the password strength rules." };
			}

			return new TenantResultModel { Success = true, Message = "The user name and password are ok." };
		}

		private TenantResultModel addSystemUserToTenant(Tenant tenant, string firstName, string lastName, string userName, string password)
		{
			var checkUser = checkFirstUserInternal(userName, password);
			if (!checkUser.Success)
				return checkUser;

			var personId = Guid.NewGuid();
			_databaseHelperWrapper.AddSystemUser(tenant.DataSourceConfiguration.ApplicationConnectionString, personId, firstName, lastName);

			var personInfo = new PersonInfo(tenant, personId);

			// todo handle passwordStrength error this is just to get it to work, the loader is in applicationdata and I don't think it is so good
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, userName, password, _currentHashFunction);
			_currentTenantSession.CurrentSession().Save(personInfo);

			addTheUserToTheTheTenantToo(_currentTenantSession.CurrentSession().Connection.ConnectionString,
				tenant.DataSourceConfiguration.ApplicationConnectionString, personInfo);
			
			return new TenantResultModel { Success = true, Message = "Created new user." };
		}

		private void addTheUserToTheTheTenantToo(string currentTenant, string superUserInTenant, PersonInfo personInfo)
		{
			var currTenant = new SqlConnectionStringBuilder(currentTenant);
			var userTenant = new SqlConnectionStringBuilder(superUserInTenant);
			if(currTenant.DataSource.Equals(userTenant.DataSource) && currTenant.InitialCatalog.Equals(userTenant.InitialCatalog))
				return;
			_databaseHelperWrapper.AddSystemUserToPersonInfo(superUserInTenant, personInfo.Id,
				personInfo.ApplicationLogonInfo.LogonName, personInfo.ApplicationLogonInfo.LogonPassword, personInfo.TenantPassword);

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

	public class TenantResultModel
	{
		public string Message { get; set; }
		public bool Success { get; set; }
		public int TenantId { get; set; }
	}

	public class AddSuperUserToTenantModel
	{
		public string Tenant { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class AddBuToTenantModel
	{
		public string Tenant { get; set; }
		public string BuName { get; set; }

	}
}