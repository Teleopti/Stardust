using Castle.Core.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PasswordManager : IPasswordManager
	{
		private readonly SignatureCreator _signatureCreator;
		private readonly TimeSpan _tokenLifeSpan = TimeSpan.FromHours(24);
		private readonly INotificationServiceClient _notification;
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly IIdUserQuery _idUserQuery;
		private readonly ICustomerDbConnector _customerDb;
		private readonly IServerConfigurationRepository _serverConfiguration;
		private readonly INow _now;
		private readonly IPasswordPolicy _passwordPolicy;
		private readonly IFindPersonInfo _findPersonInfo;
		private readonly ICheckPasswordStrength _checkPasswordStrength;
		private readonly IEnumerable<IHashFunction> _hashFunctions;
		private readonly IHashFunction _currentHashFunction;
		private readonly ITokenGenerator _tokenGenerator;

		public PasswordManager(
			SignatureCreator signatureCreator, 
			INotificationServiceClient notificationClient, 
			IApplicationUserQuery userQuery, 
			IIdUserQuery idUserQuery,
			ICustomerDbConnector custerCustomerDbConnector,
			IServerConfigurationRepository serverConfigurationRepository,
			INow now,
			IPasswordPolicy passwordPolicy,
			IFindPersonInfo findPersonInfo,
			ICheckPasswordStrength checkPasswordStrength,
			IEnumerable<IHashFunction> hashFunctions,
			IHashFunction currentHashFunction,
			ITokenGenerator tokenGenerator)
		{
			_signatureCreator = signatureCreator;
			_notification = notificationClient;
			_applicationUserQuery = userQuery;
			_idUserQuery = idUserQuery;
			_customerDb = custerCustomerDbConnector;
			_serverConfiguration = serverConfigurationRepository;
			_now = now;
			_passwordPolicy = passwordPolicy;
			_findPersonInfo = findPersonInfo;
			_checkPasswordStrength = checkPasswordStrength;
			_hashFunctions = hashFunctions;
			_currentHashFunction = currentHashFunction;
			_tokenGenerator = tokenGenerator;
		}

		public bool SendResetPasswordRequest(string userIdentifier, string baseUri)
		{
			try
			{
				var pi = _applicationUserQuery.Find(userIdentifier);
				if (pi == null)
				{
					// No user with that id found. Just return success.
					return true;
				}

				if (!_customerDb.TryGetEmailAddress(pi, out var userEmail))
				{
					return true;
				}

				var securityToken = _tokenGenerator.CreateSecurityToken(pi.Id, pi.ApplicationLogonInfo.LogonPassword);

				var resetLink = $"{baseUri}/WFM/reset_password.html?t={securityToken}";

				var templatePath = HostingEnvironment.MapPath(@"~\Areas\MultiTenancy\Core\Templates\password_reset_email_template.html");
				var mailTemplate = File.ReadAllText(templatePath);
				mailTemplate = mailTemplate.Replace("[[USER_IDENTIFIER]]", userEmail);
				mailTemplate = mailTemplate.Replace("[[RESET_LINK]]", resetLink);

				var mailMessage = new SGMailMessage
				{
					To = userEmail,
					ToFullName = "User Name",
					From = "no-reply@teleopti.com",
					FromFullName = "Teleopti Cloud Service",
					Subject = "Your Teleopti WFM password reset request",
					ContentType = "text/html",
					ContentValue = mailTemplate
				};

				var apiUrl = _serverConfiguration.Get(ServerConfigurationKey.NotificationApiEndpoint);
				var apiKey =  pi.Tenant.GetApplicationConfig(TenantApplicationConfigKey.NotificationApiKey);
				return _notification.SendEmail(mailMessage, apiUrl, apiKey);
			}
			catch (Exception)
			{
			}

			return false;
		}

		public bool Reset(string newPassword, string resetToken)
		{
			if (TryValidateResetToken(resetToken, _tokenLifeSpan.Add(TimeSpan.FromMinutes(5)), out var personId))
			{
				try
				{
					var personInfo = _findPersonInfo.GetById(personId);
					personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfo.ApplicationLogonInfo.LogonName, newPassword, _currentHashFunction);
				}
				catch (PasswordStrengthException exception)
				{
					throw new HttpException(400, "The new password does not follow the password policy.", exception);
				}
				return true;
			}

			return false;
		}

		public void Modify(Guid personId, string oldPassword, string newPassword)
		{
			if (newPassword.IsNullOrEmpty())
			{
				throw new HttpException(400, "The new password is required.");
			}

			if (oldPassword?.Equals(newPassword) ?? false)
				throw new HttpException(400, "No difference between old and new password.");

			var personInfo = _findPersonInfo.GetById(personId);
			if (personInfo == null)
				throw new HttpException(403, "Invalid user name or password.");

			var hashFunction = _hashFunctions.FirstOrDefault(x => x.IsGeneratedByThisFunction(personInfo.ApplicationLogonInfo.LogonPassword));

			if (hashFunction == null || !personInfo.ApplicationLogonInfo.IsValidPassword(_now, _passwordPolicy, oldPassword, hashFunction))
				throw new HttpException(403, "Invalid user name or password.");

			try
			{
				personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfo.ApplicationLogonInfo.LogonName, newPassword, _currentHashFunction);
			}
			catch (PasswordStrengthException passwordStrength)
			{
				throw new HttpException(400, "The new password does not follow the password policy.", passwordStrength);
			}
		}

		public bool ValidateResetToken(string resetToken)
		{
			return TryValidateResetToken(resetToken, _tokenLifeSpan, out var _);
		}

		private bool TryValidateResetToken(string tokenString, TimeSpan tokenLifeSpan, out Guid personId)
		{
			personId = Guid.Empty;
			try
			{
				var resetToken = Encryption.DecryptStringFromBase64(tokenString, EncryptionConstants.Image1, EncryptionConstants.Image2);
				var tp = resetToken.Split('|');
				if (tp.Length != 3)
				{
					return false;
				}
				var tokenTimeStamp = DateTime.Parse(tp[0]);
				var tokenUserId = Guid.Parse(tp[1]);
				var tokenSecurityStamp = tp[2];
				personId = tokenUserId;

				if (tokenTimeStamp + tokenLifeSpan < DateTime.UtcNow)
				{
					return false;
				}

				var pi = _idUserQuery.FindUserData(tokenUserId);
				
				var currentPwHash = pi.ApplicationLogonInfo.LogonPassword;
				var securityStampIsValid = _signatureCreator.Verify($"{tokenUserId}|{currentPwHash}", tokenSecurityStamp);

				if (!securityStampIsValid)
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}

	public interface ITokenGenerator
	{
		string CreateSecurityToken(Guid userId, string currentPwHash);
	}

	public class TokenGenerator : ITokenGenerator
	{
		private readonly SignatureCreator _signatureCreator;

		public TokenGenerator(SignatureCreator signatureCreator)
		{
			_signatureCreator = signatureCreator;
		}

		public string CreateSecurityToken(Guid userId, string currentPwHash)
		{
			var timeStamp = DateTime.UtcNow;
			var securityStamp = _signatureCreator.Create($"{userId}|{currentPwHash}");
			var tokenString = $"{timeStamp}|{userId}|{securityStamp}";
			var securityToken = Encryption.EncryptStringToBase64(tokenString, EncryptionConstants.Image1, EncryptionConstants.Image2);
			return securityToken;
		}
	}
}