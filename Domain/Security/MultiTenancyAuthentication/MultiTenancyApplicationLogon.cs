﻿using Teleopti.Ccc.Domain.Repositories;
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
			var nhibConfig = result.DataSource;
			if (!string.IsNullOrEmpty(result.DataSourceEncrypted))
				nhibConfig = decryptNhib(result.DataSourceEncrypted);

			logonModel.SelectedDataSourceContainer = getDataSorce(nhibConfig, applicationData);
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

		private string decryptNhib(string encryptedNhib)
		{
			return Encryption.DecryptStringFromBase64(encryptedNhib, EncryptionConstants.Image1, EncryptionConstants.Image2);
		}

		IDataSourceContainer getDataSorce(string nhibConfig, IApplicationData applicationData)
		{
			var datasource = applicationData.CreateAndAddDataSource(nhibConfig);

			return new DataSourceContainer(datasource,_repositoryFactory,null,AuthenticationTypeOption.Application);
		}

	}
}