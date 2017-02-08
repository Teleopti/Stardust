﻿@WFM
Feature: AgentGroup
	As a resource planner 
	I need to generate a plan (schedule&opt) for my part of the organization,
		so that Stockholm can be planned by 4-weeks and Paris by monthly periods,
		so that I can plan for Customer Support and Lisa can plan for Sales,
		so that John can plan for Invoice in London,
		so that Fixed agents in London have exactly 2 DOs off per week.

@OnlyRunIfEnabled('Wfm_ResourcePlanner_AgentGroup_42871')
Scenario: Create agent group
	Given there is a site named 'Site 1'
	And there is a site named 'Site 2'
	And there is a skill named 'Skill 1'
	And there is a team named 'Team 1' on 'Site 1'
	And there is a team named 'Team 2' on 'Site 2'
	And I have a role with
	| Field                     | Value            |
	| Name                      | Resource Planner |
	| Access to team            | Team 1, Team 2   |
	| Access to resourceplanner | true             |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team 1     |
	 | Start Date | 2015-01-21 |
	 | Skill      | Skill 1    |
	And John King has a person period with
	 | Field      | Value      |
	 | Team       | Team 1     |
	 | Start Date | 2015-01-21 |
	When I am viewing create agent group page
	And I input agent group name 'AgentGroup 1'
	And I select the team 
	 | Field  |
	 | Team 1 |
	 | Team 2 |
	 And I select the skill 
	 | Field   |
	 | Skill 1 |
	And I save agent group
	Then I should see 'AgentGroup 1' in the agent group list


