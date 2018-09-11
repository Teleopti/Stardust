using System;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Polly;
using Polly.Retry;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    public interface ILicenseStatusRepositories
    {
        int NumberOfActiveAgents();
        ILicenseStatusXml LicenseStatus { get; }
		void SaveLicenseStatus(string value);
        ILicenseService XmlLicenseService(int numberOfActiveAgents);
    }

    public class LicenseStatusRepositories : ILicenseStatusRepositories
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
		
        public LicenseStatusRepositories(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
			
		}

		public int NumberOfActiveAgents()
		{
			var retryPolicy = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(5, i => TimeSpan.FromMilliseconds(100));
			return retryPolicy.Execute(runNumberOfActiveAgents);
		}

		private int  runNumberOfActiveAgents()
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var rep = _repositoryFactory.CreatePersonRepository(uow);
				return rep.NumberOfActiveAgents();
			}
		}

		public ILicenseStatusXml LicenseStatus
        {
            get
            {
                using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var rep = _repositoryFactory.CreateLicenseStatusRepository(uow);
                    ILicenseStatus status = rep.LoadAll().FirstOrDefault();
	                return status == null ? new LicenseStatusXml() : new LicenseStatusXml(XDocument.Parse(status.XmlString));
                }
            }
        }

        public void SaveLicenseStatus(string value)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var rep = _repositoryFactory.CreateLicenseStatusRepository(uow);
                var status = new LicenseStatus { XmlString = value };
                rep.Add(status);
                uow.PersistAll();
            }
        }

        public ILicenseService XmlLicenseService(int numberOfActiveAgents)
        {
	        using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
	        {
						return new XmlLicenseServiceFactory().Make(_repositoryFactory.CreateLicenseRepository(uow), numberOfActiveAgents);
	        }
            
        }
    }
}