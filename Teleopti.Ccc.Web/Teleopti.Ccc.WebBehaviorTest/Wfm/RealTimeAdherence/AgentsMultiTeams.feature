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
	When  I view Real time adherence sites
	And I click the site checkbox for 'London'
	And I click the site checkbox for 'Paris'
	And I click 'open'
	Then I should see real time agent name for 'Pierre Baldi'
	And I should see real time agent name for 'Ashley Andeen'

Scenario: See agents for multiple teams
	Given there is a site named 'Stockholm'
	And there is a team named 'Täby' on site 'Stockholm'
	And there is a team named 'Garnisonen' on site 'Stockholm'
	And I have a role with full access
	And Pierre Baldi has a person period with
	 | Field      | Value      |
	 | Team       | Täby       |
	 | Start Date | 2014-01-21 |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Garnisonen |
	 | Start Date | 2014-01-21 |
	When I view Real time adherence for teams on site 'Stockholm'
	And I click the team checkbox for 'Täby'
	And I click the team checkbox for 'Garnisonen'
	And I click 'open'
	Then I should see real time agent name for 'Pierre Baldi'
	And I should see real time agent name for 'Ashley Andeen'
