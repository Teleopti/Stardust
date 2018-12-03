using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Util;

namespace Teleopti.Ccc.Domain.Security
{
	public class DataTokenManager : IDataTokenManager
	{
		private readonly SignatureCreator _signatureCreator;
		private readonly IPersonFinderReadOnlyRepository _personFinder;
		private readonly ILoggedOnUser _loggonUser;

		public DataTokenManager(ILoggedOnUser loggonUser,
			IPersonFinderReadOnlyRepository personFinder,
			SignatureCreator signatureCreator)
		{
			_loggonUser = loggonUser;
			_personFinder = personFinder;
			_signatureCreator = signatureCreator;
		}

		public SignedArgument<PersonApplicationLogonInputModel> GetTokenForPersistApplicationLogonNames(PersonApplicationLogonInputModel updateMessage)
		{
			var idsInMessageToChange = updateMessage.People.Select(x => x.PersonId);
			var isValidRequest = _personFinder.ValidatePersonIds(idsInMessageToChange.ToList(), DateOnly.Today, _loggonUser.CurrentUser().Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess);
			if (!isValidRequest)
			{
				return null;
			}

			updateMessage.TimeStamp = DateTime.UtcNow.ToString();
			updateMessage.Intent = DefinedRaptorApplicationFunctionForeignIds.PeopleAccess;

			var signedMessage = new SignedArgument<PersonApplicationLogonInputModel>
			{
				Body = updateMessage,
				Signature = _signatureCreator.Create(updateMessage.ToJson())
			};

			return signedMessage;
		}

		public SignedArgument<PersonIdentitiesInputModel> GetTokenForPersistIdentities(PersonIdentitiesInputModel updateMessage)
		{
			var idsInMessageToChange = updateMessage.People.Select(x => x.PersonId);
			var isValidRequest = _personFinder.ValidatePersonIds(idsInMessageToChange.ToList(), DateOnly.Today, _loggonUser.CurrentUser().Id.GetValueOrDefault(), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess);
			if (!isValidRequest)
			{
				return null;
			}

			updateMessage.TimeStamp = DateTime.UtcNow.ToString();
			updateMessage.Intent = DefinedRaptorApplicationFunctionForeignIds.PeopleAccess;

			var signedMessage = new SignedArgument<PersonIdentitiesInputModel>
			{
				Body = updateMessage,
				Signature = _signatureCreator.Create(updateMessage.ToJson())
			};

			return signedMessage;
		}
	}
}
