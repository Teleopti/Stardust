@OnlyRunIfEnabled('WfmTeamSchedule_ShowShiftCategory_39796')
Feature: ShiftCategory
	In order to find the mismatched shift
	As a team leader
	I want to see the shift category
Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	And there is a shift category with
    | Field      | Value |
    | Name       | Day   |
    | Short name | DY    |
    | Color      | Green |
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
	| EndTime        | 2016-10-09 17:00 |
	| Shift category | Day              |
	| Activity       | Phone            |

Scenario: Show shift category
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-09'
	Then I should see a shift category named 'DY'

@OnlyRunIfEnabled('WfmTeamSchedule_ModifyShiftCategory_39797')
Scenario: Should be able to change shift category via label
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I click on a shift category label
	And I set shift category as 'Night'
	And I apply the new shift category
	Then I should see a shift category named 'NT'

# Scenario: Should be able to change shift category via command menu