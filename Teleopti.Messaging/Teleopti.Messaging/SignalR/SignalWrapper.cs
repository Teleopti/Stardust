using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	[CLSCompliant(false)]
	public class SignalWrapper : ISignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly IHubConnectionWrapper _hubConnection;
		private int _retryCount;
		private bool _isRunning;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(SignalWrapper));
		private static readonly object LockObject = new object();
		
		public SignalWrapper(IHubProxy hubProxy, IHubConnectionWrapper hubConnection)
		{
			_isRunning = false;
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public Task NotifyClients(Notification notification)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("NotifyClients", notification);
					startTask.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							Logger.Error("An error happened when notifying.", t.Exception.GetBaseException());
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when notifying.", exception);
				}
			}
			return emptyTask();
		}

		public Task NotifyClients(IEnumerable<Notification> notifications)
		{
			if (verifyStillConnected())
			{
				try
				{
					return _hubProxy.Invoke("NotifyClientsMultiple", notifications);
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when notifying multiple.", exception);
				}
			}
			return emptyTask();
		}

		private bool verifyStillConnected()
		{
			if (_retryCount > 3)
			{
				return false;
			}

			lock (LockObject)
			{
				if (_hubConnection.State != ConnectionState.Connected)
				{
					try
					{
						startHubConnection();
						_retryCount = 0;
						return true;
					}
					catch (Exception ex)
					{
						//Suppress! Already logged upon startup for general failures.
						Logger.Error("An error happened when verifying that we still are connected.", ex);
						_retryCount++;
						return false;
					}
				}
			}
			return true;
		}

		public void StartHub()
		{
			startHubConnection();
		}

		private void startHubConnection()
		{
			// lås
			try
			{
				Exception exception = null;
				_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
				var startTask = _hubConnection.Start();
				startTask.ContinueWith(t =>
				                       	{
				                       		if (t.IsFaulted && t.Exception != null)
				                       		{
												exception = t.Exception.GetBaseException();
												Logger.Error("An error happened when starting hub connection.", exception);
				                       		}
				                       	}, TaskContinuationOptions.OnlyOnFaulted);
				
				if (!startTask.Wait(TimeSpan.FromSeconds(30)))
				{
					exception = new InvalidOperationException("Could not start message broker client within 30 seconds.");
				}
				if (exception!=null)
				{
					throw exception;
				}

				_isRunning = true;
				_hubConnection.Closed += () => { _isRunning = false; };
				_hubConnection.Reconnected += () => { _isRunning = true; };
				_hubConnection.Error += e => { _isRunning = false; };
			}
			catch (AggregateException aggregateException)
			{
				Logger.Error("An error happened when starting hub connection.", aggregateException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", aggregateException);
			}
			catch (SocketException socketException)
			{
				Logger.Error("An error happened when starting hub connection.", socketException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", socketException);
			}
			catch (InvalidOperationException exception)
			{
				Logger.Error("An error happened when starting hub connection.", exception);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", exception);
			}
		}

		public Task AddSubscription(Subscription subscription)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("AddSubscription", subscription);
					startTask.ContinueWith(t =>
					{
						if (t.IsFaulted && t.Exception != null)
						{
							Logger.Error("An error happened when adding subscription.", t.Exception.GetBaseException());
						}
					}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when adding subscriptions.", exception);
				}
			}
			return emptyTask();
		}

		private static Task emptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			return tcs.Task;
		}

		public Task RemoveSubscription(string route)
		{
			if (verifyStillConnected())
			{
				var startTask = _hubProxy.Invoke("RemoveSubscription", route);
				startTask.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
					{
						Logger.Error("An error happened when removing subscription.", t.Exception.GetBaseException());
					}
				}, TaskContinuationOptions.OnlyOnFaulted);
				return startTask;
			}
			return emptyTask();
		}

		public void StopHub()
		{
			if (_hubConnection != null && _hubConnection.State==ConnectionState.Connected)
			{
				try
				{
					_hubConnection.Stop();
					_isRunning = false;
				}
				catch (Exception ex)
				{
					Logger.Error("An error happened when stopping connection.", ex);
				}
			}
		}

		public bool IsInitialized()
		{
			return _hubProxy != null && _isRunning;
		}
	}

	[CLSCompliant(false)]
	public interface IHubConnectionWrapper
	{
		ConnectionState State { get; }
		ICredentials Credentials { get; set; }
		Task Start();
		void Stop();
		event Action Closed;
		event Action Reconnected;
		event Action<Exception> Error;
		IHubProxy CreateHubProxy(string hubName);
	}

	[CLSCompliant(false)]
	public class HubConnectionWrapper : IHubConnectionWrapper
	{
		private readonly HubConnection _hubConnection;

		public HubConnectionWrapper(HubConnection hubConnection)
		{
			_hubConnection = hubConnection;
		}

		public ConnectionState State { get { return _hubConnection.State; } }
		public ICredentials Credentials { get { return _hubConnection.Credentials; } set { _hubConnection.Credentials = value; } }

		public Task Start()
		{
			return _hubConnection.Start();
		}

		public void Stop()
		{
			_hubConnection.Stop();
		}

		public event Action Closed
		{
			add { _hubConnection.Closed += value; }
			remove { _hubConnection.Closed -= value; }
		}

		public event Action Reconnected
		{
			add { _hubConnection.Reconnected += value; }
			remove { _hubConnection.Reconnected -= value; }
		}

		public event Action<Exception> Error
		{
			add { _hubConnection.Error += value; }
			remove { _hubConnection.Error -= value; }
		}

		public IHubProxy CreateHubProxy(string hubName)
		{
			return _hubConnection.CreateHubProxy(hubName);
		}
	}
}