@ignore
Feature: Add intraday absence
	In order to keep track of persons absences
	As a team leader
	I want to add that an agent is absent for a part of his shift
	
Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And there is a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	| Modify Absence             | true                |
	| View confidential          | true                |
	And there is a role with
	| Field                      | Value                       |
	| Name                       | Anywhere Team Green and Red |
	| Access to team             | Team green,Team red         |
	| Access to Anywhere         | true                        |
	| View unpublished schedules | true                        |
	| Modify Absence             | true                        |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-15 |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green   |
	| Start date | 2013-11-15 |
	And 'Ashley Andeen' has a person period with
	| Field      | Value      |
	| Team       | Team red   |
	| Start date | 2013-11-15 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there are absences
	| Name            | Color | Confidential |
	| Illness         | Red   | false        |
	| Vacation        | Blue  | false        |
	| Mental disorder | Pink  | true         |



Scenario: Cannot see add intraday absence button if no permission
	Given I have a role with
	| Field                      | Value                 |
	| Name                       | Cannot Modify Absence |
	| Access to team             | Team green            |
	| Access to Anywhere         | true                  |
	| View unpublished schedules | true                  |
	| Modify Absence             | false                 |
	When I view schedules for 'Team green' on '2013-11-15'
	And I select any schedule activity for 'Pierre Baldi'
	Then I should not see 'add intraday absence' button
	
Scenario: View form
	Given I have the role 'Anywhere Team Green'
	When I view schedules for 'Team green' on '2013-11-15'
	And I select any schedule activity for 'Pierre Baldi'
	And I click 'add intraday absence'
	Then I should see the add intraday absence form
	
Scenario: View team mates schedules
	Given I have the role 'Anywhere Team Green and Red'
	When I view person schedules add intraday absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	Then I should see schedule for 'John King' in team mates schedules
	And I should not see schedule for 'Ashley Andeen' in team mates schedules

Scenario: View absence types in alphabetical order
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add intraday day absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	Then I should see absence type 'Illness' before 'Mental disorder'
	And I should see absence type 'Mental disorder' before 'Vacation'

Scenario: Cannot view confidential absence types if no permission
	Given I have a role with
	| Field                      | Value                    |
	| Name                       | Cannot view confidential |
	| Access to team             | Team green               |
	| Access to Anywhere         | true                     |
	| View unpublished schedules | true                     |
	| Modify Absence             | true                     |
	| View confidential          | false                    |
	When I view person schedules add intraday day absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	Then I should not see absence type 'Mental disorder'
	
Scenario: Add intraday absence on late night shift ??


Scenario: Default times adding intraday absence on shift today
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-15 13:20'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday day absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	Then I should see the add intraday absence form with
	| Field        | Value   |
	| Absence type | Illness |
	| Start time   | 13:20   |
	| End time     | 17:00   |

Scenario: Default times adding intraday absence on shift tomorrow
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-14 08:00'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday day absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	Then I should see the add intraday absence form with
	| Field        | Value   |
	| Start time   | 11:00   |
	| End time     | 12:00   |
