using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SdkTestClientWeb.Sdk;

namespace SdkTestClientWeb
{
    public partial class OrgTree : System.Web.UI.Page
    {
        private TeleoptiOrganizationService _sdk;
        private ApplicationFunctionDto _applicationFunctionDto;

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                _applicationFunctionDto = new ApplicationFunctionDto();
                _applicationFunctionDto.FunctionPath = "Raptor/Global/ViewUnpublishedSchedules";
                buildTree();
            }
        }

        private void buildTree()
        {
            _sdk = ServiceFactory.Organisation();
            organisationTreeView.Nodes.Clear();
            ICollection<SiteDto> sites = _sdk.GetSites(_applicationFunctionDto, DateTime.Now.ToUniversalTime(), true);
            foreach (var siteDto in sites)
            {
                TreeNode node = new TreeNode(siteDto.DescriptionName);

                organisationTreeView.Nodes.Add(node);
                AddTeamsToSite(siteDto, node);
                node.Expanded = false;
            }
        }

        private void AddTeamsToSite(SiteDto site, TreeNode node)
        {
            ICollection<TeamDto> teams = _sdk.GetTeams(site, _applicationFunctionDto, DateTime.Now.ToUniversalTime(), true);
            foreach (var team in teams)
            {
                TreeNode teamNode = new TreeNode(team.Description);
                node.ChildNodes.Add(teamNode);
                AddPersonsToTeam(team, teamNode);
                teamNode.Expanded = false;
            }
        }

        private void AddPersonsToTeam(TeamDto team, TreeNode node)
        {
            ICollection<PersonDto> persons = _sdk.GetPersonsByTeam(team, _applicationFunctionDto, DateTime.Now.ToUniversalTime(), true);
            foreach (var person in persons)
            {
                TreeNode personNode = new TreeNode(person.Name);
                personNode.NavigateUrl = "Schedule.aspx?PersonID=" + person.Id;
                node.ChildNodes.Add(personNode);
            }
        }
    }
}
