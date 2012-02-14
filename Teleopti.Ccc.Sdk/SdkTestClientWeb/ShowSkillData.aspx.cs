using System;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public partial class ShowSkillData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindActivities();
            }
        }
        private void BindActivities()
        {
            TeleoptiForecastingService forecasting = ServiceFactory.Forecaster();

            //ScenarioDto scenarioDto = new ScenarioDto();
            //scenarioDto.Id = "E21D813C-238C-4C3F-9B49-9B5E015AB432";
            SkillDto skillDto = new SkillDto();
            skillDto.Id = "7D044840-233F-437D-8DC5-9B5E015AB704";
            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = new DateTime(2009, 2, 3);
            dateOnlyDto.DateTimeSpecified = true;

            SkillDayDto[] skillData = forecasting.GetSkillData( dateOnlyDto, "Arabian Standard Time");
            _skillDataGrid.DataSource = skillData;
            _skillDataGrid.DataBind();
        }
    }
}
