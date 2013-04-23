using System;
using System.Collections.Generic;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Client
{
	public interface ISessionStateProvider
	{
		bool IsLoggedIn { get; }
		PersonDto LoggedOnPerson { get; }
		BusinessUnitDto BusinessUnit { get; }
		DataSourceDto DataSource { get; }
		string Password { get; }
	}

	public class SdkServiceClient : IDisposable
	{
        private readonly IDictionary<Type,IDisposable> _factories = new Dictionary<Type, IDisposable>();

		private ITeleoptiCccLogOnService _logOnServiceClient;
		private ITeleoptiSchedulingService _schedulingService;
		private ITeleoptiOrganizationService _organizationService;
		private ITeleoptiCccSdkInternal _internalService;
        private readonly object ChannelLock = new object();

		private bool IsInitialized { get { return _logOnServiceClient != null; } }

		private void Initialize()
		{
			_logOnServiceClient = CreateChannel<ITeleoptiCccLogOnService>();
		}

		private void InitializeIfNeeded()
		{
			if (!IsInitialized)
				Initialize();
		}

		private void CheckState()
		{
			InitializeIfNeeded();
		}

		public ITeleoptiCccLogOnService LogOnServiceClient
		{
			get
			{
				CheckState();
                
				return _logOnServiceClient;
			}
		}

		public ITeleoptiSchedulingService SchedulingService
		{
			get
			{
			    CheckState();
			    return _schedulingService ?? (_schedulingService = CreateChannel<ITeleoptiSchedulingService>());
			}
		}

		public ITeleoptiOrganizationService OrganizationService
		{
			get
			{
			    CheckState();
			    return _organizationService ?? (_organizationService = CreateChannel<ITeleoptiOrganizationService>());
			}
		}

        public ITeleoptiCccSdkInternal TeleoptiInternalService
        {
            get
            {
                CheckState();
                return _internalService ?? (_internalService = CreateChannel<ITeleoptiCccSdkInternal>());
            }
        }

        private T CreateChannel<T>()
        {
            lock (ChannelLock)
            {
                IDisposable factory;
                ChannelFactory<T> channelFactory;
                if (_factories.TryGetValue(typeof (T), out factory))
                {
                    channelFactory = (ChannelFactory<T>) factory;
                }
                else
                {
                    channelFactory = new ChannelFactory<T>(typeof(T).Name);
                    _factories.Add(typeof(T),channelFactory);
                }

                return channelFactory.CreateChannel();
            }
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
			/*
            if (_logOnServiceClient != null)
				_logOnServiceClient.Dispose();
			if (_schedulingService != null)
				_schedulingService.Dispose();
			if (_organizationService != null)
				_organizationService.Dispose();
            if (_internalService != null)
                _internalService.Dispose();
            */
		    foreach (var disposable in _factories)
		    {
		        disposable.Value.Dispose();
		    }
            _factories.Clear();
		}

	}
}