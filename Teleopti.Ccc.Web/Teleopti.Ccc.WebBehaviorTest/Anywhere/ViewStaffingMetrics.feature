Feature: View daily staffing metrics
	In order to make future staffing decisions
	As a team leader
	I want to see the staffing metrics for skills

Background:
	Given there is a team with
	| Field | Value      |
	| Name  | Team green |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	And there is a skill with
	| Field    | Value        |
	| Name     | Direct Sales |
	| Activity | Phone        |
	And there is a skill with
	| Field    | Value         |
	| Name     | Channel Sales |
	| Activity | Phone         |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'Pierre Baldi' has a person period with
	| Field      | Value        |
	| Team       | Team green   |
	| Start date | 2013-04-08   |
	| Skill      | Direct Sales |
	And there are shift categories
	| Name |
	| Day  |

# Bug 25359 re-enable with PBI 25562
@ignore
Scenario: View staffing metrics
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Start time     | 2013-04-09 09:00 |
	| End time       | 2013-04-09 16:00 |
	| Shift category | Day              |
	| Activity       | Phone            |
	And there is a forecast with
	| Field                    | Value        |
	| Skill                    | Direct Sales |
	| Date                     | 2013-04-09   |
	| Hours                    | 8            |
	| Service level seconds    | 20           |
	| Service level percentage | 80           |
	| Open from                | 09:00        |
	| Open to                  | 16:00        |
	When I view group schedules staffing metrics for '2013-04-09' and 'Direct Sales'
	Then I should see staffing metrics for skill 'Direct Sales' with
	| Field                   | Value  |
	| Forecasted hours        | 11.50  |
	| Scheduled hours         | 7.0    |
	| Difference hours        | -4.49  |
	| Difference percentage   | -39.10 |
	| Estimated service level | 0.00   |
# Bug 25359 re-enable with PBI 25562
@ignore
Scenario: Push staffing metrics changes

	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Start time     | 2013-09-09 09:00 |
	| End time       | 2013-09-09 16:00 |
	| Shift category | Day              |
	| Activity       | Phone            |
	And there is a forecast with
	| Field                    | Value        |
	| Skill                    | Direct Sales |
	| Date                     | 2013-09-09   |
	| Hours                    | 8            |
	| Service level seconds    | 20           |
	| Service level percentage | 80           |
	| Open from                | 09:00        |
	| Open to                  | 16:00        |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |
	| Color | Red      |
	When I view group schedules staffing metrics for '2013-09-09' and 'Direct Sales'
	Then I should see staffing metrics for skill 'Direct Sales' with
	| Field                   | Value  |
	| Forecasted hours        | 11.50  |
	| Scheduled hours         | 7.0    |
	| Difference hours        | -4.49  |
	| Difference percentage   | -39.10 |
	| Estimated service level | 0.00   |
	When 'Martin Fowler' adds an absence for 'Pierre Baldi' with
	| Field      | Value            |
	| Name       | Vacation         |
	| Start time | 2013-09-09 00:00 |
	| End time   | 2013-09-10 00:00 |
	Then I should see staffing metrics for skill 'Direct Sales' with
	| Field                   | Value   |
	| Forecasted hours        | 11.50   |
	| Scheduled hours         | 0.0     |
	| Difference hours        | -11.49  |
	| Difference percentage   | -100.00 |
	| Estimated service level | 0.00    |

@OnlyRunIfEnabled('MyTeam_StaffingMetrics_25562')
Scenario: View skill selection
	Given I have the role 'Anywhere Team Green'
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-09   |
	And there is a forecast with
	| Field    | Value         |
	| Skill    | Channel Sales |
	| Date     | 2013-04-09    |
	When I view group schedules staffing metrics for '2013-04-09'
	Then I should be able to select skills
	| Skill         |
	| Direct Sales  |
	| Channel Sales |
	
@OnlyRunIfEnabled('MyTeam_StaffingMetrics_25562')
Scenario: Remember skill selection when changing date
	Given I have the role 'Anywhere Team Green'
	And there is a forecast with
	| Field | Value         |
	| Skill | Channel Sales |
	| Date  | 2013-04-09    |
	And there is a forecast with
	| Field | Value        |
	| Skill | Direct Sales |
	| Date  | 2013-04-09   |
	And there is a forecast with
	| Field | Value         |
	| Skill | Channel Sales |
	| Date  | 2013-04-10    |
	And there is a forecast with
	| Field | Value        |
	| Skill | Direct Sales |
	| Date  | 2013-04-10   |
	And I am viewing group schedules staffing metrics for '2013-04-09' and 'Direct Sales'
	When  I select date '2013-04-10'
	Then I should see staffing metrics for skill 'Direct Sales'

@OnlyRunIfEnabled('MyTeam_StaffingMetrics_25562')
Scenario: Remember skill selection when changing team
	Given there is a team with
	| Field | Value      |
	| Name  | Team other |
	And 'Ashley Andeen' has a person period with
	| Field      | Value        |
	| Team       | Team other   |
	| Start date | 2013-04-08   |	
	And there is a forecast with
	| Field    | Value         |
	| Skill    | Channel Sales |
	| Date     | 2013-04-09    |
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-09   |
	And there is a role with
	| Field                      | Value                         |
	| Name                       | Anywhere Team Green And Other |
	| Access to team             | Team green, Team other        |
	| Access to Anywhere         | true                          |
	| View unpublished schedules | true                          |
	And I have the role 'Anywhere Team Green And Other'	
	And I am viewing group schedules staffing metrics for 'Team green' and '2013-04-09' and 'Direct Sales'
	When I select team 'Team other'
	Then I should see staffing metrics for skill 'Direct Sales'
