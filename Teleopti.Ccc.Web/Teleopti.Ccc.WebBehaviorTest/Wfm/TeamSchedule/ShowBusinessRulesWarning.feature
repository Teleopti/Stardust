@WFM
@OnlyRunIfEnabled('WfmTeamSchedule_ShowNightlyRestWarning_39619')
Feature: Show business rules warning
	As a team leader
	I want to see warnings on business rules

Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I am american
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
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

@OnlyRunIfDisabled('WfmTeamSchedule_FilterValidationWarnings_40110')
Scenario: Should be able to see business rule warnings
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I switch on show warnings toggle
	Then I should see business rule warning

@OnlyRunIfEnabled('WfmTeamSchedule_FilterValidationWarnings_40110')
Scenario: Should see no warnings if the validation rule type is not set to be viewable
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I switch on show warnings toggle
	And I open teamschedule setting panel
	And I choose not to view 'NewNightlyRestRuleName' validation result
	Then I should not see business rule warning

@OnlyRunIfEnabled('WfmTeamSchedule_FilterValidationWarnings_40110')
Scenario: Should see the warnings if the validation rule type is set to be viewable
	When I view wfm team schedules
	And I set schedule date to '2016-10-10'
	And I select a site "The site"
	And I searched schedule with keyword 'Team green'
	And I click button to search for schedules
	And I switch on show warnings toggle
	And I open teamschedule setting panel
	And I choose to view 'NewNightlyRestRuleName' validation result
	Then I should see business rule warning