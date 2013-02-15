@ignore
Feature: View agent schedule
	In order to know how an agent in my team should work today
	As a team leader
	I want to see the schedule for the agent
	
Background:
	Given there is a role with
	| Field                      | Value                   |
	| Name                       | Full access to Anywhere |
	| Access to Anywhere         | true                    |
	| View unpublished schedules | true                    |
	And there is a team with
	| Field | Value            |
	| Name  | Team green	   |
	Given there is a team member with
	| Field        | Value        |
	| Name         | Pierre Baldi |
	| TerminalDate | 2012-12-31   |
	And there is a person period for 'Pierre Baldi' with
	| Field           | Value            |
	| Team            | Team green		 |
	| StartDate       | 2012-12-01       |
	And there is an activity with
	| Field | Value |
	| Name  | Lunch |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |

Scenario: View agent schedule
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 08:00        |
	| EndTime        | 17:00        |
	| Activity       | Phone        |
	| LunchStartTime | 11:30        |
	| LunchEndTime   | 12:15        |
	| LunchActivity  | Lunch        |
	When I view schedules for '2012-12-02'
	And I click on schedule for 'Pierre Baldi'
	Then I should only see agent schedule for 'Pierre Baldi'
	
Scenario: View agent schedule with night shift from yesterday
	Given I am a team leader for 'Team green' with role 'Full access to Anywhere'
	And there is a shift with
	| Field          | Value        |
	| Person         | Pierre Baldi |
	| Date           | 2012-12-02   |
	| StartTime      | 20:00        |
	| EndTime        | 1.04:00      |
	| Activity       | Phone        |
	| LunchStartTime | 23:30        |
	| LunchEndTime   | 1.00:15      |
	| LunchActivity  | Lunch        |
	When I view schedules for '2012-12-03'
	And I click on schedule for 'Pierre Baldi'
	Then I should only see agent schedule for 'Pierre Baldi'
