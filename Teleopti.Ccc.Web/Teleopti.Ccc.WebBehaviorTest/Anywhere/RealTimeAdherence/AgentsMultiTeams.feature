Feature: Real time adherence agents in multiple teams
	In order to easier find the agent to blame
	As a real time analyst
	I want to see who is currently not adhering to the schedule
	
Background:
	Given there is a switch

Scenario: Should be able to see agents for multiple sites
	Given there is a site named 'Paris'
	And there is a team named 'Team Paris' on site 'Paris'
	And there is a site named 'London'
	And there is a team named 'Team London' on site 'London'
	And I have a role with
	| Field                                  | Value                  |
	| Name                                   | Real time analyst      |
	| Access to site                         | Paris,London           |
	| Access to team                         | Team Paris,Team London |
	| Access to real time adherence overview | True                   |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Team Paris |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value       |
	 | Team       | Team London |
	 | Start Date | 2014-01-21  |
	When I view Real time adherence overview
	And I click the site checkbox for 'London'
	And I click the site checkbox for 'Paris'
	And I click 'open'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'

Scenario: Should be able to see agents for multiple teams
	Given there is a site named 'Stockholm'
	And there is a team named 'Täby' on site 'Stockholm'
	And there is a team named 'Garnisonen' on site 'Stockholm'
	And I have a role with
	| Field                                  | Value             |
	| Name                                   | Real time analyst |
	| Access to team                         | Täby, Garnisonen  |
	| Access to real time adherence overview | True              |
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Täby       |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Garnisonen |
	 | Start Date | 2014-01-21 |
	 When I view Real time adherence for site 'Stockholm'
	And I click the team checkbox for 'Täby'
	And I click the team checkbox for 'Garnisonen'
	And I click 'open'
	Then I should see agent status for 'Pierre Baldi'
	And I should see agent status for 'Ashley Andeen'
