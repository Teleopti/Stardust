Feature: View team schedule
	In order to know how my team should work today
	As a team leader
	I want to see the schedules for the team
	
Background:
	Given there is a role with
	| Field | Value                    |
	| Name  | Full access to Admin web |
	And there is a team with
	| Field | Value            |
	| Name  | Team green |
	Given there is a team member with
	| Field        | Value        |
	| Name         | Pierre Baldi |
	| TerminalDate | 2012-12-31   |
	And there is a person period for 'Pierre Baldi' with
	| Field           | Value            |
	| Team            | Team green |
	| StartDate       | 2012-12-01       |
	And there is an activity with
	| Field | Value |
	| Name  | Lunch |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |

Scenario: View team schedule
	Given I am a team leader for 'Team green' with role 'Full access to Admin web'
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
	Then I should see schedule for 'Pierre Baldi'

	@ignore
Scenario: View only my team's schedule
	Given I am an agent in a team with access to the whole site
	And I have a shift today
	And I have a colleague
	And My colleague has a shift today
	And I have a colleague in another team
	And The colleague in the other team has a shift today
	When I view team schedule
	Then I should see my schedule
	And I should see my colleague's schedule
	And I should not see the other colleague's schedule

	@ignore
Scenario: View team schedule, day off
	Given I am an agent in a team
	And I have a colleague
	And My colleague has a dayoff today
	When I view team schedule
	Then I should see my colleague's day off

	@ignore
Scenario: View team schedule, absence 
	Given I am an agent in a team
	And I have a colleague
	And My colleague has an absence today
	When I view team schedule
	Then I should see my colleague's absence

	@ignore
Scenario: View team schedule, no shift
	Given I am an agent in a team
	And I have a colleague
	When I view team schedule
	Then I should see myself without schedule
	And I should see my colleague without schedule

	@ignore
Scenario: Can't see confidential absence
	Given I am an agent in a team
	And I have a colleague
	And My colleague has a confidential absence
	When I view team schedule
	Then I should see my colleague's schedule
	And I should not see the absence's color
 
 @ignore
Scenario: Can't see the team schedule tab without permission 
	Given I am an agent with no access to team schedule
	When I am viewing an application page
	Then I should not see the team schedule tab

	@ignore
Scenario: Can't navigate to team schedule without permission 
	Given I am an agent with no access to team schedule
	When I am viewing an application page
	And I navigate to the team schedule
	Then I should see an error message

	@ignore
Scenario: Can't see colleagues schedule without permission
	Given I am an agent in a team with access only to my own data
	And I have a colleague
	And My colleague has a shift today
	When I view team schedule
	Then I should not see my colleagues schedule
