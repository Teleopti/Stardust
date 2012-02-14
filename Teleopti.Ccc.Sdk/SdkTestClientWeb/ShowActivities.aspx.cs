using System;
using System.Collections.Generic;
using System.Web.UI;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public partial class ShowActivities : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                bindActivities();
            }
        }

        private void bindActivities()
        {
            TeleoptiSchedulingService sdk = ServiceFactory.SdkService();

            ICollection<ActivityDto> activities = sdk.GetActivities(new LoadOptionDto{LoadDeleted = false,LoadDeletedSpecified = true});
            activityGrid.DataSource = activities;
            activityGrid.DataBind();
        }
    }
}
