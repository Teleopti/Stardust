using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.Security
{
	public interface IDataTokenManager
	{
		SignedArgument<PersonApplicationLogonInputModel> GetTokenForPersistApplicationLogonNames(PersonApplicationLogonInputModel updateMessage);
		SignedArgument<PersonIdentitiesInputModel> GetTokenForPersistIdentities(PersonIdentitiesInputModel updateMessage);
	}
}