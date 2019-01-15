@RTA
Feature: Agents in multiple teams
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule
	
Scenario: See agents for multiple sites
	Given there is a site named 'Paris'
	And there is a team named 'Team Paris' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Team London' on site 'London'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Team Paris |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value       |
	 | Team       | Team London |
	 | Start Date | 2014-01-21  |
	Given I am viewing real time adherence on sites 'London, Paris' only
	When I click the toggle to see all agents
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'

Scenario: See agents for multiple teams
	Given there is a site named 'Stockholm'
	And there is a team named 'Täby' on site 'Stockholm'
	And there is a team named 'Garnisonen' on site 'Stockholm'
	And there is a team named 'Sollentuna' on site 'Stockholm'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Täby       |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Garnisonen |
	 | Start Date | 2014-01-21 |
	Given I am viewing real time adherence on teams 'Täby, Garnisonen'
	When I click the toggle to see all agents
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
