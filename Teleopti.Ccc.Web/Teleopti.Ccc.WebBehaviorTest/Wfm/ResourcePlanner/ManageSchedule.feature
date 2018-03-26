@WFM
Feature: ManageSchedule
	As a resource planner
	I want to be able to archive schedules from default scenario to another scenario
	I want to be able to import schedules from another scenario to default scenario

@OnlyRunIfEnabled('Wfm_ArchiveSchedule_41498')
Scenario: Run archiving for one agent
	Given there is a scenario
	| Field          | Value        |
	| Name           | To           |
	| Business Unit  | BusinessUnit |
	| Extra scenario | true         |
	And there is a site named 'Site 1'
	And there is a team named 'Team 1' on 'Site 1'
	And I have a role with
	| Field                     | Value        |
	| Name                      | Archive Role |
	| Access to team            | Team 1       |
	| Access to resourceplanner | true         |
	| Archive schedules         | true         |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team 1     |
	 | Start Date | 2015-01-21 |
	When I am viewing archive schedule page
	And I wait 3 seconds to allow tracking to be setup
	And I select 'To' as to scenario
	And I pick new dates
	And I select the team 'Team 1'
	And I run archiving
	And I confirm to run archiving
	Then I should get a success message
