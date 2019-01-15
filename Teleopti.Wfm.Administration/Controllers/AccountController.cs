using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
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
	/// <summary>
	/// Changes to this file may affect third party connections, i.e. Twillio, TalkDesk etc.
	/// Please contact CloudOps when changes are required and made. 
	/// </summary>
	[TenantTokenAuthentication]
	public class AccountController : ApiController
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IHangfireCookie _hangfireCookie;
		private readonly IHashFunction _currentHashFunction;
		private readonly IEnumerable<IHashFunction> _hashFunctions;
		private readonly AdminAccessTokenRepository _adminAccessTokenRepository;

		public AccountController(ICurrentTenantSession currentTenantSession, IHangfireCookie hangfireCookie, 
			IHashFunction currentHashFunction, IEnumerable<IHashFunction> hashFunctions, AdminAccessTokenRepository adminAccessTokenRepository)
		{
			_currentTenantSession = currentTenantSession;
			_hangfireCookie = hangfireCookie;
			_currentHashFunction = currentHashFunction;
			_hashFunctions = hashFunctions;
			_adminAccessTokenRepository = adminAccessTokenRepository;
		}

		[OverrideAuthentication]
		[HttpPost]
		[TenantUnitOfWork]
		[Route("Login")]
		public virtual JsonResult<LoginResult> Login(LoginModel model)
		{
			if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
				return Json(new LoginResult { Success = false, Message = "Both user name and password must be provided." });

			string sql = "SELECT Id, Name, Password FROM Tenant.AdminUser WHERE Email=@email";
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
							
							var hashFunction = _hashFunctions.FirstOrDefault(x => x.IsGeneratedByThisFunction(passwordHash));
							if (hashFunction == null)
								break;
							if (!hashFunction.Verify(model.Password, passwordHash))
								break;

							if (_currentHashFunction.GetType() != hashFunction.GetType())
							{
								// Update the password with new hash
								var user = _currentTenantSession.CurrentSession().Get<TenantAdminUser>(id);
								user.Password = _currentHashFunction.CreateHash(model.Password);
								_currentTenantSession.CurrentSession().Save(user);
							}
							
							reader.Close();
							var accessToken = _adminAccessTokenRepository.CreateNewToken(id, sqlConnection);

							_hangfireCookie.SetHangfireAdminCookie(userName, model.UserName);
							var valuesString = $"\"tokenKey\":\"{accessToken}\",\"user\":\"{userName}\",\"id\":{id}";
							valuesString = "{" + valuesString + "}";
							var cook = new HttpCookie("WfmAdminAuth", Uri.EscapeDataString(valuesString)) { HttpOnly = true };

							if (HttpContext.Current == null)
								return Json(new LoginResult {Success = true, Id = id, UserName = userName, AccessToken = accessToken});
							if (HttpContext.Current.Request.IsSecureConnection)
							{
								cook.Secure = true;
							}
							HttpContext.Current.Response.Cookies.Add(cook);

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
			HttpContext.Current.Response.Cookies.Add(new HttpCookie("WfmAdminAuth"){Expires = DateTime.Now.AddDays(-1)});

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
					Message = "Password must be at least 6 characters."
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
				var encryptedPassword = _currentHashFunction.CreateHash(model.Password);
				//var token = _currentHashFunction.CreateHash(model.Email);
				var user = new TenantAdminUser
				{
					Email = model.Email,
					Name = model.Name,
					Password = encryptedPassword,
					AccessToken = ""
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

			var hashFunction = _hashFunctions.FirstOrDefault(x => x.IsGeneratedByThisFunction(user.Password));
			if (hashFunction == null)
				return Json(new UpdateUserResultModel { Success = false, Message = "The password is not correct. (2)" });

			if (!hashFunction.Verify(model.OldPassword, user.Password))
				return Json(new UpdateUserResultModel { Success = false, Message = "The password is not correct." });

			if (!model.NewPassword.Equals(model.ConfirmNewPassword))
				return Json(new UpdateUserResultModel { Success = false, Message = "The new password and confirm password does not match." });
			if (model.NewPassword.Length < 6)
				return Json(new UpdateUserResultModel { Success = false, Message = "Password must be at least 6 characters." });
			try
			{
				var encryptedPassword = _currentHashFunction.CreateHash(model.NewPassword);

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
			return _currentTenantSession.CurrentSession().GetNamedQuery("loadNumberOfTenantUsers").UniqueResult<long>() > 0L;
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

		[OverrideAuthentication]
		[HttpGet]
		[Route("LoggedInUser")]
		public virtual JsonResult<UserModel> LoggedInUser()
		{
			if (HttpContext.Current.Request.Cookies.AllKeys.Contains("WfmAdminAuth"))
			{
				var cook = HttpContext.Current.Request.Cookies["WfmAdminAuth"];
				var value = Uri.UnescapeDataString(cook.Value);
				var obj = System.Web.Helpers.Json.Decode<cookieValues>(value);
				return Json(new UserModel {Name = obj.user, Id = obj.id, Token = obj.tokenKey});
			}

			return Json(new UserModel());
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

	public class UserModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Token { get; set; }
	}
}