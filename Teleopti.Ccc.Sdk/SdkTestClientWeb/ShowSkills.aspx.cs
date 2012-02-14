using System;
using System.Collections.Generic;
using System.Web.UI;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public partial class ShowSkills : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                bindSkills();
            }
        }

        private void bindSkills()
        {
            TeleoptiForecastingService sdk = ServiceFactory.Forecaster();

            ICollection<SkillDto> skills = sdk.GetSkills();
            skillGrid.DataSource = skills;
            skillGrid.DataBind();
        }

    }
}
