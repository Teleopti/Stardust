using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ManagerStarter));

		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

		public void Stop()
		{
			QuitEvent.Set();
		}
        
        public void Start(ManagerConfiguration managerConfiguration)
        {

            QuitEvent.WaitOne();

        }
        
    }

}