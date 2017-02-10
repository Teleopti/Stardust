@RTA
Feature: Monitor agents based on permitted site/teams only
	In order to give correct access to team leads
	so that team leads can see their team in RTA
	As a real time analyst
	I need all RTA views to be fully permission based


Background: Access permitted site/teams only
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a skill named 'Sales' with activity 'Phone'
	And there is a site named 'Paris'
	And there is a site named 'Denver'
	And there is a team named 'Red' on site 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Team Linda' on site 'Denver'
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Red        |
	 | Start Date | 2017-02-10 |
	 | Skill      | Sales      |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Green      |
	 | Start Date | 2017-02-10 |
	 | Skill      | Sales      |
	And Linda Lee has a person period with
	 | Field      | Value      |
	 | Team       | Team Linda |
	 | Start Date | 2017-02-10 |
	 | Skill      | Sales      |
	And Pierre Baldi has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2017-02-10 08:00 |
	| End time                 | 2017-02-10 17:00 |
	And Ashley Andeen has a shift with
	| Field                    | Value            |
	| Activity                 | Phone            |
	| Start time               | 2017-02-10 08:00 |
	| End time                 | 2017-02-10 17:00 |
	And Linda Lee has a shift with
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
	And there is a rule with 
	| Field       | Value    |
	| Name        | Adhering |
	| Activity    | Phone    |
	| Phone state | Ready    |
	| Is alarm    | false    |

@ignore
Scenario: See how many agents with a specific skill that are in alarm for permitted site
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Site leader |
	 | Access to site                         | Paris       |
	 | Access to real time adherence overview | True        |
	
	And the time is '2017-02-10 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And 'Ashley Andeen' sets her phone state to 'LoggedOut'
	And 'Linda Lee' sets her phone state to 'LoggedOut'
	When I view Real time adherence sites
	Then I should see site 'Paris' with 2 of 2 agents in alarm
	And I should not see site 'Denver'


@ignore
Scenario: See how many agents with a specific skill that are in alarm for a permitted team on site
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	
	And the time is '2017-02-10 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And 'Ashley Andeen' sets her phone state to 'LoggedOut'
	And 'Linda Lee' sets her phone state to 'LoggedOut'
	When I view Real time adherence for skill 'Sales' for teams on site 'Paris'
	Then I should see team 'Red' with 1 of 1 agents in alarm
	And I should not see team 'Team Linda'

@ignore
Scenario: Monitor agents by skills
	Given I have a role with
	 | Field                                  | Value       |
	 | Name                                   | Team leader |
	 | Access to team                         | Red         |
	 | Access to real time adherence overview | True        |
	
	And the time is '2017-02-10 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And 'Ashley Andeen' sets her phone state to 'LoggedOut'
	And 'Linda Lee' sets her phone state to 'LoggedOut'
	When I view Real time adherence sites
	And I click 'select skill'
	And I select skill 'Sales'
	Then I should see agent 'Pierre Baldi' with state 'LoggedOut'
	And I should not see agent 'Ashley Andeen'
	And I should not see agent 'Linda Lee'

@ignore
Scenario: Quickly change agent selection for skill, site and team
    Given I have a role with
	| Field                                  | Value       |
	| Name                                   | Team leader |
	| Access to team                         | Red         |
	| Access to real time adherence overview | True        |
	
	And the time is '2017-02-10 08:00:00'
	When I view Real time adherence sites
	And I click 'details'
	Then I should see site 'Paris' in 'select organizaion'
	And I should see team 'Red' in 'select organizaion'
	And I should not see team 'Green' in 'select organizaion'
	And I should not see site 'Denver' in 'select organizaion'