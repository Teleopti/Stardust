using System;
using System.Web.UI;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public partial class Schedule : Page
    {
        private TeleoptiSchedulingService _sdk;
        private TeleoptiOrganizationService _orgService;
        protected void Page_Load(object sender, EventArgs e)
        {
            _sdk = ServiceFactory.SdkService();
            _orgService = ServiceFactory.Organisation();
            if(!IsPostBack)
            {
                bindSchedule();
            }
        }

        private void bindSchedule()
        {
            SchedulePartDto schedulePart = loadSchedulePartDto();
            if (schedulePart.PersonAssignmentCollection.Length>0)
            {
                layersGrid.DataSource = schedulePart.PersonAssignmentCollection[0].MainShift.LayerCollection;
                layersGrid.DataBind();
                shiftCat.Text = "Shiftkategori: " + schedulePart.PersonAssignmentCollection[0].MainShift.ShiftCategoryId;    
            }
            else
            {
                shiftCat.Text = "Här var det tomt";
            }
            if(schedulePart.PersonAbsenceCollection.Length>0)
            {
                abs.Text = "Frånvaro:" + schedulePart.PersonAbsenceCollection[0].AbsenceLayer.Absence.Name;
            }
            else
            {
                abs.Text = "Ingen frånvaro";
            }
            PersonAccountDto[] personAccs = getPersonAccounts();
            PersonAcc.Text = "Antal person account =: " + personAccs.Length;
            if (personAccs.Length > 0)
            {
                AccountInfo.Text = "Account period: " + personAccs[0].Period.StartDate.DateTime + " - " + personAccs[0].Period.EndDate.DateTime + " Balance: " + personAccs[0].LatestCalculatedBalance;
            }
        }

        private  PersonAccountDto[] getPersonAccounts()
        {
            string personId = Request.QueryString["PersonID"];
            if (string.IsNullOrEmpty(personId))
                Response.Redirect("OrgTree.aspx");
            PersonDto personDto = new PersonDto { Id = personId };
            DateTime dateTime = new DateTime(2009, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateOnlyDto d = new DateOnlyDto();
            d.DateTime = dateTime;
            d.DateTimeSpecified = true;

            return _orgService.GetPersonAccounts(personDto, d);
        }
        private SchedulePartDto loadSchedulePartDto()
        {
            string personId = Request.QueryString["PersonID"];
            if (string.IsNullOrEmpty(personId))
                Response.Redirect("OrgTree.aspx");
            PersonDto personDto = new PersonDto {Id = personId};
            DateTime dateTime = new DateTime(2009, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateOnlyDto d = new DateOnlyDto();
            d.DateTime = dateTime;
            d.DateTimeSpecified = true;
            return _sdk.GetSchedulePart(personDto, d, "Central Pacific Standard Time");
        }

        protected void changeSchedule_Click(object sender, EventArgs e)
        {
            TeleoptiSchedulingService scheduling = ServiceFactory.SdkService();
            SchedulePartDto schedulePart = loadSchedulePartDto();
            schedulePart.PersonAssignmentCollection[0].MainShift.LayerCollection[0].Period.UtcEndTime =
                schedulePart.PersonAssignmentCollection[0].MainShift.LayerCollection[0].Period.UtcEndTime.AddMinutes(-1);
            scheduling.SaveSchedulePart(schedulePart);

            bindSchedule();
        }

        protected void nonChangedSchedule_Click(object sender, EventArgs e)
        {
            TeleoptiSchedulingService scheduling = ServiceFactory.SdkService();
            SchedulePartDto schedulePart = loadSchedulePartDto();
            scheduling.SaveSchedulePart(schedulePart);
        }
    }
}
