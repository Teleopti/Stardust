using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security
{
	public class MultiTenancyApplicationLogon : IApplicationLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public MultiTenancyApplicationLogon(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public AuthenticationResult Logon(ILogonModel logonModel)
		{
			// fejkar att vi fått datasource från web service
			var allAppContainers =
				logonModel.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application)
					.ToList();
			var dataSourceName = "Teleopti WFM";
			var personId = new Guid("10957AD5-5489-48E0-959A-9B5E015B2B5C");

			logonModel.SelectedDataSourceContainer = allAppContainers.Where(d => d.DataSourceName.Equals(dataSourceName)).FirstOrDefault();
			// if null error
			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{

				logonModel.SelectedDataSourceContainer.SetUser(_repositoryFactory.CreatePersonRepository(uow).LoadOne(personId));
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		} 
	}
}