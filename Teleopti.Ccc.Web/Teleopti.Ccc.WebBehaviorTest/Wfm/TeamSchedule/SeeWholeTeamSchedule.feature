@OnlyRunIfEnabled('WfmTeamSchedule_SetAgentsPerPage_36230')
Feature: SeeWholeTeamSchedule
	As a team leader works in a big team
	I want to be able to see schedules of my whole team

Background: 
	Given there is a team with
	| Field | Value      |
	| Name  | My Team |
	And there is a role with
	| Field                       | Value      |
	| Name                        | TeamLeader |
	| Access to team              | My Team    |
	| Access to Outbound          | true       |
	| Access to Wfm Team Schedule | true       |
	| View unpublished schedules  | true       |
	And I have a person period with
	| Field      | Value      |
	| Team       | My Team    |
	| Start date | 2013-09-26 |

Scenario: Can see page size dropdown box
	Given I have the role 'TeamLeader'
	When I am view wfm team schedules
	Then I can see page size picker

Scenario: Can change page size
	Given I have the role 'TeamLeader'
	And I have page size '50' in personal setting
	When I am view wfm team schedules
	Then I can see page size picker
	And page size picker is filled with '50'