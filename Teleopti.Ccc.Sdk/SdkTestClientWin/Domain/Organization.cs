﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class Organization
    {
        private readonly IList<Site> _siteColl = new List<Site>();
        private readonly IList<Agent> _agentColl = new List<Agent>();
		private readonly IList<PersonPeriod> _personPeriod = new List<PersonPeriod>();

		

        public ReadOnlyCollection<Site> SiteCollection
        {
            get { return new ReadOnlyCollection<Site>(_siteColl); }
        }

        public ReadOnlyCollection<Agent> AgentCollection
        {
            get { return new ReadOnlyCollection<Agent>(_agentColl); }
        }

    	public ReadOnlyCollection<PersonPeriod> PersonPeriodCollection
    	{
    		get {return new ReadOnlyCollection<PersonPeriod>(_personPeriod);}
    	}

    	public IList<PersonPeriod> GetPersonPeriods(Agent agent)
    	{
    		return _personPeriod.Where(personPeriod => personPeriod.PersonId == agent.Dto.Id).ToList();
    	}

        public void Load(ServiceApplication sdkService, ApplicationFunctionDto applicationFunctionDto, DateTime date)
        {
           
            IList<SiteDto> sites = new List<SiteDto>(sdkService.OrganizationService.GetSites(applicationFunctionDto, date, true));
            foreach (SiteDto siteDto in sites)
            {
                Site site = new Site(siteDto);
                _siteColl.Add(site);
                IList<TeamDto> teams = new List<TeamDto>(sdkService.OrganizationService.GetTeams(siteDto, applicationFunctionDto, date, true));
                foreach (TeamDto teamDto in teams)
                {
                    Team team = site.CreateAndAddTeam(teamDto);
                    IList<PersonDto> agents = new List<PersonDto>(sdkService.OrganizationService.GetPersonsByTeam(teamDto, applicationFunctionDto, date, true));
                    foreach (PersonDto personDto in agents)
                    {
                        Agent agent = team.CreateAndAddAgent(personDto);
                        _agentColl.Add(agent);
                    }
                }
            }
        }

		public void LoadAllPersonPeriods(ServiceApplication sdkService)
		{

			var startDateOnlyDto = new DateOnlyDto();
			var endDateOnlyDto = new DateOnlyDto();

			startDateOnlyDto.DateTime = DateTime.MinValue;
			endDateOnlyDto.DateTime = DateTime.MaxValue;

			var personPeriodLoadOptionDto = new PersonPeriodLoadOptionDto();
			var loadOptionDto = new LoadOptionDto {LoadDeleted = true, LoadDeletedSpecified = true};

			personPeriodLoadOptionDto.LoadAll = true;
			personPeriodLoadOptionDto.LoadAllSpecified = true;
			startDateOnlyDto.DateTimeSpecified = true;
			endDateOnlyDto.DateTimeSpecified = true;

			ICollection<PersonDto> personDtos = sdkService.OrganizationService.GetPersons(true, true);
			ICollection<PersonPeriodDetailDto> personPeriodDtos = sdkService.OrganizationService.GetPersonPeriods(personPeriodLoadOptionDto, startDateOnlyDto, endDateOnlyDto);
			ICollection<ContractDto> contractDtos = sdkService.OrganizationService.GetContracts(loadOptionDto);
			ICollection<PartTimePercentageDto> partTimePercentageDtos = sdkService.OrganizationService.GetPartTimePercentages(loadOptionDto);
			ICollection<ContractScheduleDto> contractScheduleDtos = sdkService.OrganizationService.GetContractSchedules(loadOptionDto);

			foreach (var personPeriod in from personPeriodDto2 in personPeriodDtos
			                             let personDto = GetPersonDto(personDtos, personPeriodDto2.PersonId)
			                             let contractDto = GetContractDto(contractDtos, personPeriodDto2.ContractId)
			                             let partTimePercentageDto = GetPartTimePercentageDto(partTimePercentageDtos, personPeriodDto2.PartTimePercentageId)
			                             let contractScheduleDto = GetContractScheduleDto(contractScheduleDtos, personPeriodDto2.ContractScheduleId)
			                             select new PersonPeriod(personPeriodDto2, personDto, contractDto, partTimePercentageDto, contractScheduleDto))
			{
				_personPeriod.Add(personPeriod);
			}
		}

		private static PersonDto GetPersonDto(IEnumerable<PersonDto> personDtos, string personId)
		{
			return personDtos.FirstOrDefault(personDto => personDto.Id == personId);
		}

		private static ContractDto GetContractDto(IEnumerable<ContractDto> contractDtos, string contractId)
		{
			return contractDtos.FirstOrDefault(contractDto => contractDto.Id == contractId);
		}

		private static PartTimePercentageDto GetPartTimePercentageDto(IEnumerable<PartTimePercentageDto> partTimePercentageDtos, string partTimePercentageId)
		{
			return partTimePercentageDtos.FirstOrDefault(partTimePercentageDto => partTimePercentageDto.Id == partTimePercentageId);
		}

		private static ContractScheduleDto GetContractScheduleDto(IEnumerable<ContractScheduleDto> contractScheduleDtos, string contractScheduleId)
		{
			return contractScheduleDtos.FirstOrDefault(contractScheduleDto => contractScheduleDto.Id == contractScheduleId);
		}

        public void DrawTree(TreeView treeView)
        {
            treeView.BeginUpdate();
            treeView.Nodes["All"].Nodes.Clear();
            foreach (Site site in SiteCollection)
            {
                TreeNode siteNode = new TreeNode(site.Dto.DescriptionName);
                siteNode.Tag = site;
                treeView.Nodes["All"].Nodes.Add(siteNode);
                foreach (Team team in site.TeamColl)
                {
                    TreeNode teamNode = new TreeNode(team.Dto.Description);
                    teamNode.Tag = team;
                    siteNode.Nodes.Add(teamNode);
                    foreach (Agent agent in team.AgentColl)
                    {
                        TreeNode agentNode = new TreeNode(agent.Dto.Name);
                        agentNode.Tag = agent;
                        teamNode.Nodes.Add(agentNode);
                    }
                }
            }

            treeView.EndUpdate();
        }

        public ReadOnlyCollection<Agent> SelectedAgents(TreeNode selectedNode)
        {
            if(selectedNode == null)
                return new ReadOnlyCollection<Agent>(new List<Agent>());

            List<Agent> retList = new List<Agent>();

            if(selectedNode.Tag is Agent)
            {
                retList.Add((Agent)selectedNode.Tag);
                return new ReadOnlyCollection<Agent>(retList);
            }

            if(selectedNode.Tag is Team)
            {
                retList = new List<Agent>(((Team)selectedNode.Tag).AgentColl);
                return new ReadOnlyCollection<Agent>(retList);
            }

            if(selectedNode.Tag is Site)
            {
                Site site = (Site) selectedNode.Tag;
                foreach (Team team in site.TeamColl)
                {
                    retList.AddRange(team.AgentColl);
                }
                return new ReadOnlyCollection<Agent>(retList);
            }

            return new ReadOnlyCollection<Agent>(_agentColl);
        }
    }
}
