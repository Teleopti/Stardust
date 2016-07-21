@OnlyRunIfEnabled('WfmTeamSchedule_ModifyShiftCategory_39797')
@OnlyRunIfEnabled('WfmTeamSchedule_ShowShiftCategory_39796')
Feature: Change Shift Category
	As a team leader
	I want to be able to set/change the shift category on agents' shifts

Background: 
	Given there is a site named 'The site'
	And there is a team named 'Team green' on site 'The site'
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
	And there is a shift category with
    | Field      | Value |
    | Name       | Night   |
    | Short name | NT    |
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
	| StartTime      | 2016-10-10 01:00 |
	| EndTime        | 2016-10-10 18:00 |
	| Shift category | Day              |
	| Activity       | Phone            |

Scenario: Should be able to change shift category via label
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-10-10'
	And I click on a shift category label
	And I set shift category as 'Night'
	And I apply the new shift category
	Then I should see a shift category named 'NT'

# Scenario: Should be able to change shift category via command menu