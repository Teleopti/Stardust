Feature: Add intraday absence on shift
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
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-15 |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |
	And there are activities
	| Name        | Color  |
	| Phone       | Green  |
	| Lunch       | Yellow |
	And there are absences
	| Name            | Color | Confidential |
	| Illness         | Red   | false        |
	| Vacation        | Blue  | false        |
	| Mental disorder | Pink  | true         |
	
Scenario: View form
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view schedules for 'Team green' on '2013-11-15'
	And I select any schedule activity for 'Pierre Baldi'
	And I click 'add intraday absence' in shift menu
	Then I should see the add intraday absence form

Scenario: Default times today
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-15 13:20'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	Then I should see the add intraday absence form with
	| Field        | Value   |
	| Start time   | 13:30   | 
	| End time     | 17:00   |

Scenario: Default times tomorrow
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-14 08:00'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	Then I should see the add intraday absence form with
	| Field        | Value   |
	| Start time   | 11:00   |
	| End time     | 12:00   |

Scenario: Add on shift
	Given I have the role 'Anywhere Team Green'	
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Illness |
	| Start time | 15:00   |
	| End time   | 17:00   |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value |
	| Start time | 15:00 |
	| End time   | 17:00 |
	| Color      | Red   |

Scenario: Add after midnight on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 22:00 |
	| End time       | 2013-11-16 04:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Activity   | Illness |
	| Start time | 01:00   |
	| End time   | 03:00   |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value |
	| Start time | 01:00 |
	| End time   | 03:00 |
	| Color      | Red   |

Scenario: Add cross midnight on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 22:00 |
	| End time       | 2013-11-16 04:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Activity   | Illness |
	| Start time | 23:00   |
	| End time   | 01:00   |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value |
	| Start time | 23:00 |
	| End time   | 01:00 |
	| Color      | Red   |

Scenario: Adding overlapping of shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Illness |
	| Start time | 16:00   |
	| End time   | 18:00   |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value |
	| Start time | 16:00 |
	| End time   | 17:00 |
	| Color      | Red   |

Scenario: Prevent invalid times
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Illness |
	| Start time | 15:00   |
	| End time   | 14:00   |
	Then I should see the alert 'Invalid end time'

Scenario: Prevent adding outside of shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-15 11:00 |
	| End time       | 2013-11-15 17:00 |
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-11-15'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Illness |
	| Start time | 17:00   |
	| End time   | 18:00   |
	Then I should see the alert 'Invalid Intraday Absence Times'

Scenario: Go to yesterday when select the night shift starting from yesterday
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field                         | Value            |
	| Shift category                | Night            |
	| Activity                      | Phone            |
	| Start time                    | 2013-11-15 22:00 |
	| End time                      | 2013-11-16 04:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2013-11-16 00:15 |
	| Scheduled activity end time   | 2013-11-16 00:45 |
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Night            |
	| Activity       | Phone            |
	| Start time     | 2013-11-16 22:15 |
	| End time       | 2013-11-17 04:15 |
	When I view schedules for 'Team green' on '2013-11-16'
	And I select the schedule activity for 'Pierre Baldi' with start time '00:45'
	And I click 'add intraday absence' in shift menu
	Then I should see the add intraday absence form for '2013-11-15'

Scenario: Back to viewing schedule after adding an intraday absence
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add intraday absence form for 'Pierre Baldi' in 'Team green' on '2013-04-08'
	And I input these intraday absence values
	| Field      | Value   |
	| Absence    | Illness |
	| Start time | 17:00   |
	| End time   | 18:00   |
	And I initiate 'apply'
	Then I should be viewing schedules for '2013-04-08'	