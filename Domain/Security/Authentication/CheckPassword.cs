using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class CheckPassword : ICheckPassword
    {
        private readonly IOneWayEncryption _encryption;
        private readonly ICheckBruteForce _checkBruteForce;
        private readonly ICheckPasswordChange _checkPasswordChange;

        public CheckPassword(IOneWayEncryption encryption, ICheckBruteForce checkBruteForce, ICheckPasswordChange checkPasswordChange)
        {
            _encryption = encryption;
            _checkBruteForce = checkBruteForce;
            _checkPasswordChange = checkPasswordChange;
        }

    	protected IOneWayEncryption Encryption
    	{
    		get { return _encryption; }
    	}

    	public virtual AuthenticationResult CheckLogOn(IUserDetail userDetail, string password)
        {
            AuthenticationResult result;
            if (userDetail.Person.ApplicationAuthenticationInfo.Password.Equals(Encryption.EncryptString(password)))
            {
                result = _checkPasswordChange.Check(userDetail);
            }
            else
            {
                result = handleWrongPassword(userDetail);
            }
            return result;
        }

    	private AuthenticationResult handleWrongPassword(IUserDetail userDetail)
    	{
    		AuthenticationResult result = _checkBruteForce.Check(userDetail);
    		if (!result.HasMessage)
    		{
    			result.HasMessage = true;
    			result.Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword;
    		}
    		return result;
    	}
    }

	public class CheckPasswordWithToken : CheckPassword
	{
		private readonly IPassphraseProvider _passphraseProvider;

		public CheckPasswordWithToken(IOneWayEncryption encryption, IPassphraseProvider passphraseProvider, ICheckBruteForce checkBruteForce, ICheckPasswordChange checkPasswordChange) : base(encryption, checkBruteForce, checkPasswordChange)
		{
			_passphraseProvider = passphraseProvider;
		}

		public override AuthenticationResult CheckLogOn(IUserDetail userDetail, string password)
		{
			if (Encryption.EncryptStringWithBase64(_passphraseProvider.Passphrase(),userDetail.Person.ApplicationAuthenticationInfo.ApplicationLogOnName.ToLower(CultureInfo.CurrentUICulture)).Equals(password))
			{
				return new AuthenticationResult {Person = userDetail.Person, Successful = true};
			}
			return base.CheckLogOn(userDetail, password);
		}
	}

	public interface IPassphraseProvider
	{
		string Passphrase();
	}
}