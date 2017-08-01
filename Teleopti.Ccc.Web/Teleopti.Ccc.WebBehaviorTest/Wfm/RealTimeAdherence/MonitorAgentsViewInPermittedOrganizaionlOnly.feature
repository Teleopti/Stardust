﻿@RTA
Feature: Monitor agents view based on permitted site/teams only
	As an Adherence Analyst (Team Lead/Site real-time analyst) 
	I want to see agents in a site/team based on my permissions
	In order to get an overview of the actual situation

Background: Access permitted agents on site/teams
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'SiteGreen'
	And there is a site named 'SitePink'
	And there is a team named 'TeamBlue' on site 'SiteGreen'
	And there is a team named 'TeamYellow' on site 'SiteGreen'
	And there is a team named 'TeamPink' on site 'SitePink'
	And AgentBlue has a person period with
	 | Field      | Value      |
	 | Team       | TeamBlue   |
	 | Start Date | 2017-02-10 |
	And AgentYellow has a person period with
	 | Field      | Value      |
	 | Team       | TeamYellow |
	 | Start Date | 2017-02-10 |
	And AgentPink has a person period with
	 | Field      | Value      |
	 | Team       | TeamPink   |
	 | Start Date | 2017-02-10 |
	And AgentBlue has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2017-02-10 08:00 |
	| End time                 | 2017-02-10 17:00 |
	And AgentYellow has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2017-02-10 08:00 |
	| End time                 | 2017-02-10 17:00 |
	And AgentPink has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2017-02-10 08:00 |
	| End time                 | 2017-02-10 17:00 |
	And there is a rule with 
	| Field       | Value        |
	| Name        | Not adhering |
	| Activity    | Phone        |
	| Phone state | LoggedOut    |
	| Is alarm    | true         |
	And the time is '2017-02-10 08:00:00'
	And 'AgentBlue' sets his phone state to 'LoggedOut'
	And 'AgentYellow' sets her phone state to 'LoggedOut'
	And 'AgentPink' sets her phone state to 'LoggedOut'
	
Scenario: As a Team leader - See agents that are in alarm for my site
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | TeamBlue    |
	 | Access to real time adherence overview | True        |
	When I am viewing real time adherence on site 'SiteGreen'
	Then I should see agent status for 'AgentBlue'
	And I should not see agent 'AgentYellow'
	And I should not see agent 'AgentPink'
	
Scenario: As a Team leader - Only see permitted team
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | TeamBlue    |
	 | Access to real time adherence overview | True        |
	When I view real time adherence for all agents on team 'TeamBlue'
	Then I should see agent status for 'AgentBlue'
	And I should not see agent 'AgentYellow'
	And I should not see agent 'AgentPink'
	
Scenario: As a Site manager - Only see permitted site
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Site leader |
	 | Access to site                         | SiteGreen   |
	 | Access to real time adherence overview | True        |
	When I am viewing real time adherence on site 'SiteGreen'
	Then I should see agent status for 'AgentBlue'
	And I should see agent status for 'AgentYellow'
	And I should not see agent 'AgentPink'