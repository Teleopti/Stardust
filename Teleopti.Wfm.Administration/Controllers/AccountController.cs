﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Hangfire;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class AccountController : ApiController
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IHangfireCookie _hangfireCookie;
		private readonly IHashFunction _hashFunction = new OneWayEncryption();

		public AccountController(ICurrentTenantSession currentTenantSession, IHangfireCookie hangfireCookie)
		{
			_currentTenantSession = currentTenantSession;
			_hangfireCookie = hangfireCookie;
		}

		[OverrideAuthentication]
		[HttpPost]
		[TenantUnitOfWork]
		[Route("Login")]
		public virtual JsonResult<LoginResult> Login(LoginModel model)
		{

			if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new LoginResult { Success = false, Message = "Both user name and password must be provided." });

			string sql = "SELECT Id, Name, Password, AccessToken FROM Tenant.AdminUser WHERE Email=@email";
			using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@email", model.UserName);
					var reader = sqlCommand.ExecuteReader();
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var id = reader.GetInt32(0);
							var userName = reader.GetString(1);
							var passwordHash = reader.GetString(2);
							var accessToken = reader.GetString(3);
							if (!_hashFunction.Verify(model.Password, passwordHash))
								break;

							_hangfireCookie.SetHangfireAdminCookie(userName, model.UserName);
							return Json(new LoginResult { Success = true, Id = id, UserName = userName, AccessToken = accessToken });
						}
					}
					return Json(new LoginResult { Success = false, Message = "No user found with that email and password." });
				}
			}
		}

		[OverrideAuthentication]
		[HttpPost]
		[Route("Logout")]
		public virtual IHttpActionResult Logout(LoginModel model)
		{
			_hangfireCookie.RemoveAdminCookie();
			return Ok();
		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("Users")]
		public virtual JsonResult<IList<TenantAdminUser>> Users()
		{
			return
				Json(
					(IList<TenantAdminUser>)
						_currentTenantSession.CurrentSession()
							.GetNamedQuery("loadAllTenantUsers")
							.List<TenantAdminUser>()
							.Select(t => new TenantAdminUser { Id = t.Id, Email = t.Email, Name = t.Name })
							.ToArray());
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("User")]
		public virtual JsonResult<UpdateUserModel> GetOneUser([FromBody]int id)
		{
			var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(id);
			if (user != null)
				return Json(new UpdateUserModel { Id = user.Id, Email = user.Email, Name = user.Name });

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
			return addOneUser(model);
		}

		private UpdateUserResultModel addOneUser(AddUserModel model)
		{
			if (string.IsNullOrEmpty(model.Password))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Password can't be empty."
				};
			if (model.Password.Length < 6)
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Email must be at least 6 characters."
				};
			if (string.IsNullOrEmpty(model.Email))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "Email can't be empty."
				};

			if (model.Email.ToLower().Equals("admin@company.com"))
				return new UpdateUserResultModel
				{
					Success = false,
					Message = "That email is not allowed."
				};
			// 
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
				var encryptedPassword = _hashFunction.CreateHash(model.Password);
				var token = _hashFunction.CreateHash(model.Email);
				var user = new TenantAdminUser
				{
					Email = model.Email,
					Name = model.Name,
					Password = encryptedPassword,
					AccessToken = token
				};

				_currentTenantSession.CurrentSession().Save(user);
			}
			catch (Exception exception)
			{
				_currentTenantSession.CurrentSession().Clear();
				return new UpdateUserResultModel
				{
					Success = false,
					Message = exception.InnerException?.Message ?? exception.Message
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

			if (!_hashFunction.Verify(model.OldPassword, user.Password))
				return Json(new UpdateUserResultModel { Success = false, Message = "The password is not correct." });

			if (!model.NewPassword.Equals(model.ConfirmNewPassword))
				return Json(new UpdateUserResultModel { Success = false, Message = "The new password and confirm password does not match." });

			try
			{
				var encryptedPassword = _hashFunction.CreateHash(model.NewPassword);

				user.Password = encryptedPassword;

				_currentTenantSession.CurrentSession().Save(user);

			}
			catch (Exception exception)
			{
				_currentTenantSession.CurrentSession().Clear();
				return Json(new UpdateUserResultModel
				{
					Success = false,
					Message = exception.InnerException?.Message ?? exception.Message
				});
			}

			return Json(new UpdateUserResultModel { Success = true, Message = "Successfully changed password." });
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("DeleteUser")]
		public virtual void DeleteUser([FromBody]int id)
		{
			var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(id);
			if (user != null)
				_currentTenantSession.CurrentSession().Delete(user);
		}

		[OverrideAuthentication]
		[HttpGet]
		[TenantUnitOfWork]
		[Route("HasNoUser")]
		public virtual JsonResult<bool> HasNoUser()
		{
			return Json(!hasUser());
		}

		private bool hasUser()
		{
			return _currentTenantSession.CurrentSession().GetNamedQuery("loadAllTenantUsers").List<TenantAdminUser>().Count > 0;
		}

		[OverrideAuthentication]
		[HttpPost]
		[TenantUnitOfWork]
		[Route("AddFirstUser")]
		public virtual UpdateUserResultModel AddFirstUser(AddUserModel model)
		{
			if (hasUser())
			{
				return new UpdateUserResultModel { Success = false, Message = "First user was already created." };
			}

			var result = addOneUser(model);
			if (result.Success)
			{
				var existing = _currentTenantSession
					.CurrentSession()
					.GetNamedQuery("loadAllTenants")
					.List<Tenant>();
				foreach (var tenant in existing)
				{
					tenant.Active = true;
					_currentTenantSession.CurrentSession().Save(tenant);
				}
			}
			return result;
		}

		[OverrideAuthentication]
		[HttpPost]
		[TenantUnitOfWork]
		[Route("CheckEmail")]
		public virtual JsonResult<UpdateUserResultModel> CheckEmail(CheckEmailModel checkEmailModel)
		{
			if (checkEmailModel.Email.ToLower().Equals("admin@company.com"))
				return Json(new UpdateUserResultModel { Success = false, Message = "Email not allowed." });
			TenantAdminUser another;
			var existing = _currentTenantSession
				.CurrentSession()
				.GetNamedQuery("loadAllTenantUsers")
				.List<TenantAdminUser>();

			if (checkEmailModel.Id.Equals(0))
				another = existing.FirstOrDefault(t => t.Email.ToLower().Equals(checkEmailModel.Email.ToLower()));
			else
			{
				another = existing.FirstOrDefault(
					t => t.Email.ToLower().Equals(checkEmailModel.Email.ToLower()) && t.Id != checkEmailModel.Id);
			}
			if (another != null)
				return Json(new UpdateUserResultModel { Success = false, Message = "Email already exists." });

			return Json(new UpdateUserResultModel { Success = true, Message = "Alright." });
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

	public class CheckEmailModel
	{
		public int Id { get; set; }
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