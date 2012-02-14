using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Session["CookieContainer"] = new CookieContainer();
            //Create the Service object
            TeleoptiCccLogOnService sdkService = ServiceFactory.LogonService();
            //Get the datasources
            ICollection<DataSourceDto> availableDataSources = sdkService.GetDataSources();
            //Select the first one (generally choosen by a user)
            DataSourceDto dataSource = availableDataSources.First();
            //Login to that datasource, and get the available business units
            ICollection<BusinessUnitDto> availableBusinessUnits = sdkService.LogOnApplication("demo", "demo", dataSource);
            //Select the first one (generally choosen by a user)
            BusinessUnitDto businessUnit = availableBusinessUnits.FirstOrDefault();
            //Set the business unit
            sdkService.SetBusinessUnit(businessUnit);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}