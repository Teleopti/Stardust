@RTA
Feature: Monitor agents based on permitted site/teams only
  In order to get an overview of the actual situation
  As an Adherence Analyst (Team Lead/Site real-time analyst)
  I want to see the site/team card based on my permissions

  Background: Access permitted site/teams only
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
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2017-02-10 08:00 |
	  | End time   | 2017-02-10 17:00 |
	And AgentYellow has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2017-02-10 08:00 |
	  | End time   | 2017-02-10 17:00 |
	And AgentPink has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2017-02-10 08:00 |
	  | End time   | 2017-02-10 17:00 |
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

  Scenario: As a Team leader - See how many agents that are in alarm for my site
	Given I have a role with
	  | Field                                  | Value       |
	  | Name                                   | Team leader |
	  | Access to team                         | TeamBlue    |
	  | Access to real time adherence overview | True        |
	When I view Real time adherence sites
	Then I should see site 'SiteGreen'
	And I should not see site 'SitePink'

  Scenario: As a Team leader - Only see permitted team
	Given I have a role with
	  | Field                                  | Value       |
	  | Name                                   | Team leader |
	  | Access to team                         | TeamBlue    |
	  | Access to real time adherence overview | True        |
	When I view Real time adherence for teams on site 'SiteGreen'
	Then I should see team 'TeamBlue'
	And I should not see team 'TeamYellow'

  Scenario: As a Site manager - Only see permitted site
	Given I have a role with
	  | Field                                  | Value       |
	  | Name                                   | Site leader |
	  | Access to site                         | SiteGreen   |
	  | Access to real time adherence overview | True        |
	When I view Real time adherence sites
	Then I should see site 'SiteGreen'
	And I should not see site 'SitePink'