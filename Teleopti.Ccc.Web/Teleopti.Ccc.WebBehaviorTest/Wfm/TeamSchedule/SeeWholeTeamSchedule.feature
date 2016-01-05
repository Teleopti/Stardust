@ignore
@OnlyRunIfEnabled('WfmTeamSchedule_SetAgentsPerPage_36230')
Feature: SeeWholeTeamSchedule
	As a team leader works in a big team
	I want to be able to see schedules of my whole team

Background: 
	Given I have a role with
		| Field                      | Value            |
		| Name                       | Resource Planner |
		| Access to permissions      | True             |
	And I am swedish

Scenario: Can see page size dropdown box
	When I am view wfm team schedules
	Then I can see page size picker


Scenario: Can change page size
	Given I have page size '50' in personal setting
	When I am view wfm team schedules
	Then I can see page size picker
	Then page size picker is filled with '50'