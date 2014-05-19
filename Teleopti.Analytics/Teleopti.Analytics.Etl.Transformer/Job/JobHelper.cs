using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;
using IRaptorRepository = Teleopti.Analytics.Etl.Interfaces.Transformer.IRaptorRepository;

namespace Teleopti.Analytics.Etl.Transformer.Job
{
    public class JobHelper : IJobHelper
    {
        private IRaptorRepository _repository;
        private ILogOnHelper _logHelp;
        private IMessageSender _messageSender;

        protected JobHelper(ILogOnHelper logOnHelper, IMessageSender messageSender)
        {
            _logHelp = logOnHelper;
            _messageSender = messageSender;
        }

        public JobHelper()
        {
			_logHelp = new LogOnHelper(SuperUser.UserName, SuperUser.Password, ConfigurationManager.AppSettings["nhibConfPath"]);
            _messageSender = SignalSender.MakeForEtl(ConfigurationManager.AppSettings["MessageBroker"]);
        }

        public JobHelper(IRaptorRepository repository, IMessageSender messageSender, ILogOnHelper logOnHelper) : this(logOnHelper, messageSender)
        {
            _repository = repository;
        }

        public IList<IBusinessUnit> BusinessUnitCollection
        {
            get { return _logHelp.GetBusinessUnitCollection(); }
        }

        public IRaptorRepository Repository
        {
            get { return _repository; }
        }

        public IMessageSender MessageSender
        {
            get { return _messageSender; }
        }

        public bool LogOnTeleoptiCccDomain(IBusinessUnit businessUnit)
        {
			if (!_logHelp.LogOn(businessUnit))
			{
				return false;
			}

            //Create repository when logged in to raptor domain
            _repository = new RaptorRepository(ConfigurationManager.AppSettings["datamartConnectionString"],
                                               ConfigurationManager.AppSettings["isolationLevel"]);
        	return true;
        }

        public void LogOffTeleoptiCccDomain()
        {
            _logHelp.LogOff();
            _repository = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        protected virtual void ReleaseUnmanagedResources()
        {

        }

        protected virtual void ReleaseManagedResources()
        {
            _repository = null;
            if (_logHelp != null)
                _logHelp.Dispose();
            _logHelp = null;
            _messageSender = null;
        }

    }
}