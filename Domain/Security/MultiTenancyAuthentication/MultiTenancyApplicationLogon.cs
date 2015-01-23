using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		
		public MultiTenancyApplicationLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
		}

		public AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData)
		{
			var result = _authenticationQuerier.TryLogon(logonModel.UserName, logonModel.Password);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false, 
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tennant;
			var personId = result.PersonId;

			logonModel.SelectedDataSourceContainer = getDataSorce(result.DataSourceEncrypted, applicationData);
			// if null error
			if (logonModel.SelectedDataSourceContainer == null)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = string.Format(Resources.CannotFindDataSourceWithName, dataSourceName)
				};

			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personId);
				logonModel.SelectedDataSourceContainer.SetUser(person);
				logonModel.SelectedDataSourceContainer.LogOnName = logonModel.UserName;
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}

		IDataSourceContainer getDataSorce(string encryptedNhib, IApplicationData applicationData)
		{
			var nhibConfig = Encryption.DecryptStringFromBase64(encryptedNhib, EncryptionConstants.Image1, EncryptionConstants.Image2);

			var datasource = applicationData.CreateAndAddDataSource(nhibConfig);

			return new DataSourceContainer(datasource,_repositoryFactory,null,AuthenticationTypeOption.Application);
		}

	}
}