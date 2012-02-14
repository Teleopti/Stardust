using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Teleopti.Ccc.WebBehaviorTest.Core
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class SystemLevelLock : IDisposable
	{
		private readonly string _id;
		private bool _isAquired;
		private Mutex _mutex;

		private static string GetApplicationId()
		{
			return ((GuidAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (GuidAttribute), false).GetValue(0)).Value;
		}

		public SystemLevelLock(string id)
		{
			_id = id;
			Aquire();
		}

		public SystemLevelLock() : this(GetApplicationId()) { }

		private void Aquire()
		{
			try
			{
				var mutex = GetMutex();
				_isAquired = mutex.WaitOne(TimeSpan.FromMinutes(1), false);
				if (!_isAquired)
					throw new Exception("System level mutex could not be aquired");
			}
			catch (AbandonedMutexException)
			{
				// Mutex was abandoned by another process (it probably crashed)
				// Mutex was aquired by this process instead
			}
		}

		private Mutex GetMutex() { return _mutex ?? (_mutex = MakeMutex()); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private Mutex MakeMutex()
		{
			var mutexId = string.Format("Global\\{{{0}}}", _id);
			var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
			var securitySettings = new MutexSecurity();
			securitySettings.AddAccessRule(allowEveryoneRule);
			var mutex = new Mutex(false, mutexId);
			mutex.SetAccessControl(securitySettings);
			return mutex;
		}

		private void Release()
		{
			if (_isAquired)
				_mutex.ReleaseMutex();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose() { Release(); }

	}
}