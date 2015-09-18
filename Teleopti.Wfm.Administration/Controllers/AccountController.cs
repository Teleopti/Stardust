using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class AccountController : ApiController
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private const string Salt = "adgvabar4g61qt46gv";

		public AccountController(  ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		[OverrideAuthentication]
		[HttpPost]
		[TenantUnitOfWork]
		[Route("Login")]
		public virtual JsonResult<LoginResult> Login(LoginModel model)
		{
			var hashed = encryptString(model.Password);
			
			string sql = "SELECT Id, Name, AccessToken FROM Tenant.AdminUser WHERE  Email=@email AND Password=@password";
			using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@email", model.UserName);
					sqlCommand.Parameters.AddWithValue("@password", hashed);
					var reader = sqlCommand.ExecuteReader();
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							return Json(new LoginResult { Success = true, Id = reader.GetInt32(0), UserName = reader.GetString(1), AccessToken = reader.GetString(2) });
						}
						
					}
					return Json(new LoginResult { Success = false, Message = "No user found with that email and password." });
				}
			}

		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("Users")]
		public virtual JsonResult<IList<TenantAdminUser>> Users()
		{
			return Json(_currentTenantSession.CurrentSession().GetNamedQuery("loadAllTenantUsers").List<TenantAdminUser>());
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("User")]
		public virtual JsonResult<UpdateUserModel> GetOneUser([FromBody]int id)
		{
			var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(id);
			if(user != null)
				return Json(new UpdateUserModel {Id = user.Id, Email = user.Email, Name = user.Name});

			return Json(new UpdateUserModel());
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("SaveUser")]
		public virtual UpdateUserResultModel SaveUser(UpdateUserModel model)
		{
			if (string.IsNullOrEmpty(model.Email))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Email can't be empty."
				};
			if (string.IsNullOrEmpty(model.Name))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Name can't be empty."
				};

			try
			{
				var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(model.Id);
				if (user != null)
				{
					user.Name = model.Name;
					user.Email = model.Email;
					_currentTenantSession.CurrentSession().Save(user);
				}
			}
			catch (Exception exception)
			{
				return new UpdateUserResultModel
				{
					Success = false,
					Message = exception.Message
				};
			}

			return new UpdateUserResultModel
			{
				Success = true,
				Message = "Updated the user successfully."
			};
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("AddUser")]
		public virtual UpdateUserResultModel AddUser(AddUserModel model)
		{
			if (string.IsNullOrEmpty(model.Password))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Password can't be empty."
				};
			if (string.IsNullOrEmpty(model.Email))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Email can't be empty."
				};
			if (string.IsNullOrEmpty(model.Name))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Name can't be empty."
				};
			if (!model.Password.Equals(model.ConfirmPassword))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Password and confirm password does not match."
				};

			try
			{
				var encryptedPassword = encryptString(model.Password);
				var token = encryptString(model.Email);
				var user = new TenantAdminUser {Email = model.Email, Name = model.Name, Password = encryptedPassword, AccessToken = token};
				
				_currentTenantSession.CurrentSession().Save(user);
				
			}
			catch (Exception exception)
			{
				_currentTenantSession.CurrentSession().Clear();
                return new UpdateUserResultModel
				{
					Success = false,
					Message = exception.InnerException != null ? exception.InnerException.Message : exception.Message
				};
			}

			return new UpdateUserResultModel
			{
				Success = true,
				Message = "Updated the user successfully."
			};
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("ChangePassword")]
		public virtual JsonResult<UpdateUserResultModel> ChangePassword(ChangePasswordModel model)
		{
			var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(model.Id);
			if (user == null)
				return Json(new UpdateUserResultModel { Success = false, Message = "Can not find the user." });

			var hashed = encryptString(model.OldPassword);
			if (!hashed.Equals(user.Password))
				return Json(new UpdateUserResultModel { Success = false, Message = "The password is not correct." });

			if(!model.NewPassword.Equals(model.ConfirmNewPassword))
				return Json(new UpdateUserResultModel { Success = false, Message = "The new password and confirm password does not match." });

			try
			{
				var encryptedPassword = encryptString(model.NewPassword);

				user.Password = encryptedPassword;

				_currentTenantSession.CurrentSession().Save(user);

			}
			catch (Exception exception)
			{
				_currentTenantSession.CurrentSession().Clear();
				return Json(new UpdateUserResultModel
				{
					Success = false,
					Message = exception.InnerException != null ? exception.InnerException.Message : exception.Message
				});
			}

			return Json(new UpdateUserResultModel { Success = true, Message = "Successfully changed password."});
		}

		private string encryptString(string value)
		{
			return string.Concat("###", BitConverter.ToString(hashString(value)).Replace("-", ""), "###");
		}
		private byte[] hashString(string value)
		{
			var stringValue = string.Concat(Salt, value);
			using (SHA1Managed encryptor = new SHA1Managed())
			{
				return encryptor.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
			}			
		}
	}

	public class ChangePasswordModel
	{
		public int Id { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmNewPassword { get; set; }
	}

	public class UpdateUserModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}

	public class AddUserModel
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
	}
	public class UpdateUserResultModel
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}