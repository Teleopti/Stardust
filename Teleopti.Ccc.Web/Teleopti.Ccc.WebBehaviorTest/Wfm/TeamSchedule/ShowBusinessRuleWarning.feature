@OnlyRunIfEnabled('WfmTeamSchedule_ShowNightlyRestWarning_39619')
Feature: ShowBusinessRuleWarning
	As a team leader
	I want to see warnings on business rules

Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field        | Value |
	| Name         | Phone |
	| Color        | Green |
	| In work time | True  |
	And there is a contract named 'A contract'
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'
	And 'John Smith' has a person period with
	| Field                | Value                |
	| Team                 | Team green           |
	| Start date           | 2016-01-01           |
	| Contract             | A contract           |
	And John Smith has a schedule period with 
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Type       | Week       |
	| Length     | 1          |
	And John Smith has a shift with
	| Field          | Value            |
	| StartTime      | 2016-10-09 08:00 |
	| EndTime        | 2016-10-09 23:00 |
	| Shift category | Day              |
	| Activity       | Phone            |
	And John Smith has a shift with
	| Field          | Value            |
	| StartTime      | 2016-10-10 01:00 |
	| EndTime        | 2016-10-10 18:00 |
	| Shift category | Day              |
	| Activity       | Phone            |

Scenario: Should be able to see business rule warnings
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I switch on show warnings toggle
	Then I should see business rule warning
