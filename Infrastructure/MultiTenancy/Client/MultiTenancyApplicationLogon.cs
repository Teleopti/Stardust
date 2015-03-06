﻿using NHibernate.Cfg;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IDataSourcesFactory _dataSourcesFactory;

		public MultiTenancyApplicationLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier,
			IDataSourcesFactory dataSourcesFactory)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
			_dataSourcesFactory = dataSourcesFactory;
		}

		public AuthenticationResult Logon(ILogonModel logonModel, string userAgent)
		{
			var result = _authenticationQuerier.TryApplicationLogon(logonModel.UserName, logonModel.Password, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false, 
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tenant;
			var personId = result.PersonId;
			var dataSourceCfg = result.DataSourceConfiguration;

			dataSourceCfg.ApplicationNHibernateConfig[Environment.SessionFactoryName] = dataSourceName;
			var datasource = _dataSourcesFactory.Create(dataSourceCfg.ApplicationNHibernateConfig,
				dataSourceCfg.AnalyticsConnectionString);
			logonModel.SelectedDataSourceContainer = new DataSourceContainer(datasource, _repositoryFactory, null, AuthenticationTypeOption.Application);
			// if null error
			if (logonModel.SelectedDataSourceContainer == null)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = string.Format(Resources.CannotFindDataSourceWithName, dataSourceName),
					PasswordPolicy = result.PasswordPolicy
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
				Successful = true,
				PasswordPolicy = result.PasswordPolicy
			};
		}
	}
}