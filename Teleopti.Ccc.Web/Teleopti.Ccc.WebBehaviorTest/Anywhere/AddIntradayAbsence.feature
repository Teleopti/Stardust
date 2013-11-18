@ignore
Feature: Add intraday absence
	In order to keep track of persons absences
	As a team leader
	I want to add that an agent is absent for a part of his/her shift
	
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
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-15 |
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
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



Scenario: Prevent adding if no permission
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

Scenario: Prevent selection of confidential if no permission
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

Scenario: Default times today
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

Scenario: Default times tomorrow
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

Scenario: Invalid times
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday day absence form for 'Pierre Baldi' and 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value |
	| Start time | 09:00 |
	| End time   | 10:00 |
	Then I should see the alert 'Invalid time'
	# is 16:00 to 19:00 valid?

Scenario: Add on shift
	
Scenario: Add on late night shift

Scenario: Prevent add outside shift
