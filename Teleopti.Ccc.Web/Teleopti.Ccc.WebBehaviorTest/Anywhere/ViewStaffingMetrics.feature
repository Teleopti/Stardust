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
	And I have a person period with
	| Field        | Value        |
	| Team         | Team green   |
	| Start date   | 2013-04-08   |
	| Person skill | Direct Sales |
	And there are shift categories
	| Name |
	| Day  |

Scenario: View skill selection
	Given I have the role 'Anywhere Team Green'
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-09   |
	| Hours    | 8            |
	| OpenFrom | 09:00        |
	| OpenTo   | 16:00        |
	And there is a forecast with
	| Field    | Value         |
	| Skill    | Channel Sales |
	| Date     | 2013-04-09    |
	| Hours    | 8             |
	| OpenFrom | 09:00         |
	| OpenTo   | 16:00         |
	When I view schedules for '2013-04-09'
	Then I should be able to select skills
	| Skill         |
	| Direct Sales  |
	| Channel Sales |
# Bug 25359 re-enable with PBI 25562
@ignore
Scenario: View staffing metrics
	Given I have the role 'Anywhere Team Green'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2013-04-09 09:00 |
	| EndTime        | 2013-04-09 16:00 |
	| Shift category | Day              |
	| Activity       | Phone            |
	And there is a forecast with
	| Field                  | Value        |
	| Skill                  | Direct Sales |
	| Date                   | 2013-04-09   |
	| Hours                  | 8            |
	| ServiceLevelSeconds    | 20           |
	| ServiceLevelPercentage | 80           |
	| OpenFrom               | 09:00        |
	| OpenTo                 | 16:00        |
	When I view schedules for '2013-04-09'
	And I select skill 'Direct Sales'
	Then I should see metrics for skill 'Direct Sales' with
	| Field                   | Value  |
	| Forecasted hours        | 11.50  |
	| Scheduled hours         | 7.0    |
	| Difference hours        | -4.49  |
	| Difference percentage   | -39.10 |
	| Estimated service level | 0.00   |

Scenario: Remember skill selection when changing date
	Given I have the role 'Anywhere Team Green'
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-09   |
	| Hours    | 8            |
	| OpenFrom | 09:00        |
	| OpenTo   | 16:00        |
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-10   |
	| Hours    | 8            |
	| OpenFrom | 09:00        |
	| OpenTo   | 16:00        |
	And I view schedules for '2013-04-09'
	When I select skill 'Direct Sales'
	And I select date '2013-04-10'
	Then I should see metrics for skill 'Direct Sales'

Scenario: Remember skill selection when changing team
	Given there is a team with
	| Field | Value      |
	| Name  | Team other |
	And there is a forecast with
	| Field    | Value        |
	| Skill    | Direct Sales |
	| Date     | 2013-04-09   |
	| Hours    | 8            |
	| OpenFrom | 09:00        |
	| OpenTo   | 16:00        |
	And there is a forecast with
	| Field    | Value         |
	| Skill    | Channel Sales |
	| Date     | 2013-04-09    |
	| Hours    | 8             |
	| OpenFrom | 09:00         |
	| OpenTo   | 16:00         |
	And there is a role with
	| Field                      | Value                         |
	| Name                       | Anywhere Team Green And Other |
	| Access to team             | Team green, Team other        |
	| Access to Anywhere         | true                          |
	| View unpublished schedules | true                          |
	And I have the role 'Anywhere Team Green And Other'
	And I view schedules for '2013-04-09'
	When I select skill 'Direct Sales'
	And I select team 'Team other'
	Then I should see metrics for skill 'Direct Sales'