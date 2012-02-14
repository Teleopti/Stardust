using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;
using log4net;
using log4net.Config;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.PM.PMServiceHost
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PMService : IPMService
    {
        private readonly ILog _logger;
        private List<UserDto> _clientUsersToSynchronize;

    	public PMService()
        {
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(PMService));
            _clientUsersToSynchronize = new List<UserDto>();
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public ResultDto AddUsersToSynchronize(UserDto[] users)
        {
            var resultDto = new ResultDto {Success = false};

            if (_clientUsersToSynchronize == null)
            {
                var pme = new PmSynchronizeException("Cannot add users to be synchronized since field variable _clientUsersToSynchronize is not instantiated!");
                resultDto.ErrorMessage = pme.Message;
                resultDto.ErrorType = pme.GetType().ToString();
                return resultDto;
            }

            if (users != null && users.Length > 0)
            {
                _logger.DebugFormat("Before adding users to synchronize. Users to sync: {0}. Users to add: {1}", _clientUsersToSynchronize.Count, users.Length);
                _clientUsersToSynchronize.AddRange(users);
                resultDto.Success = true;
                _logger.DebugFormat("After adding users to synchronize. Users to sync: {0}. ", _clientUsersToSynchronize.Count);
            }
            else
            {
                _logger.Debug("User arary is either null contains no elements in call to AddUsersToSynchronize(UserDto[] users).");
            }

            return resultDto;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public ResultDto SynchronizeUsers(string olapServer, string olapDatabase)
        {
            var resultDto = new ResultDto { Success = false };

            try
            {
                checkIdentity();

                using (var analyzerProxy = new ClientProxy(olapServer, olapDatabase))
                {
                    if (analyzerProxy.EnsureDataSourceExists())
                    {
                        var synchronizer = new Synchronizer(_clientUsersToSynchronize, new UserHandler(analyzerProxy));

                        resultDto = synchronizer.SynchronizeUsers();
                    }
                    else
                    {
                        var pme =
                            new PmSynchronizeException(string.Concat("Could not create Analyzer data source '", olapServer, "'."));
                        resultDto.ErrorMessage = pme.Message;
                        resultDto.ErrorType = pme.GetType().ToString();
                    }
                    analyzerProxy.LogOffUser();
                }
            }
            catch (PmSynchronizeException pme)
            {
                _logger.ErrorFormat("Exception in PMService.SynchronizeUsers() of type PmSynchronizeException thrown. Message: {0}", pme.Message);
                resultDto.ErrorMessage = pme.Message;
                resultDto.ErrorType = pme.GetType().ToString();
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Exception in PMService.SynchronizeUsers() thrown. Message: {0}", e.Message);
                resultDto.ErrorMessage = e.Message;
                resultDto.ErrorType = e.GetType().ToString();
            }

            return resultDto;
        }
		
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public void ResetUserLists()
        {
            _clientUsersToSynchronize = new List<UserDto>();
        }

        private void checkIdentity()
        {
            WindowsIdentity callerWindowsIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            if (callerWindowsIdentity == null)
            {
                throw new InvalidOperationException("The caller cannot be mapped to a Windows Identity");
            }
            _logger.DebugFormat("Before contacting Analyzer the client caller is: '{0}'", callerWindowsIdentity.Name);
        }
    }
}
