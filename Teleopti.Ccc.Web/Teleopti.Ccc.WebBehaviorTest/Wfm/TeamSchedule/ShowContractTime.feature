@OnlyRunIfEnabled('WfmTeamSchedule_ShowContractTime_38509')
Feature: ShowContractTime
	As a team leader
	I want to see agents' contract times

Background:
	Given I am american
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a role with
	| Field                         | Value          |
	| Name                          | Wfm Team Green |
	| Access to everyone            | True           |
	| Access to Wfm MyTeam Schedule | true           |
	And 'John Smith' has a person period with
		| Field                | Value                |
		| Team                 | Team green           |
		| Start date           | 2016-01-01           |
	And 'John Smith' has a workflow control set publishing schedules until '2016-12-01'	
	And John Smith has a schedule period with 
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Type       | Week       |
	| Length     | 1          |
	And there is a shift category named 'Day'	
	And John Smith has a shift with
	| Field                 | Value            |
	| StartTime             | 2016-01-01 08:00 |
	| EndTime               | 2016-01-01 18:00 |
	| Shift category		| Day	           |

Scenario: Should be able to see contract time
	When I view wfm team schedules
	And I searched schedule with keyword 'Team green' and schedule date '2016-01-01'
	Then I should see contract time of '10:00'
