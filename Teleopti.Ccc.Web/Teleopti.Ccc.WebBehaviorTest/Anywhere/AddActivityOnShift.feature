@ignore
Feature: Add activity
	In order to assign work to an working agent
	As a team leader
	I want to add activity to an existing shift

Background:
	Given there is a team with
	| Field | Value            |
	| Name  | Team green       |
	And I have a role with
	| Field                      | Value               |
	| Name                       | Anywhere Team Green |
	| Access to team             | Team green          |
	| Access to Anywhere         | true                |
	| View unpublished schedules | true                |
	And 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	| Color | Green |
	And there is an activity with
	| Field | Value  |
	| Name  | Lunch  |
	| Color | Yellow |
	
Scenario: Prevent assigning if no permission

Scenario: View form
Given I have the role 'Anywhere Team Green'
	When I view schedules for 'Team green' on '2013-11-18'
	And I select any schedule activity for 'Pierre Baldi'
	And I initiate 'assign activity'
	Then I should see the assign activity form

Scenario: View team mates schedules
Given I have the role 'Anywhere Team Green'
And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-18'
	Then I should see schedule for 'John King' in team mates schedules

Scenario: Default times today
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-18 13:20'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-18'
	Then I should see the assign activity form with
	| Field      | Value |
	| Start time | 13:20 |
	| End time   | 17:00 |

Scenario: Default times tomorrow
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-18 13:20'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 11:00 |
	| End time       | 2013-11-19 17:00 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-19'
	Then I should see the assign activity form with
	| Field      | Value |
	| Start time | 11:00 |
	| End time   | 12:00 |

# If its easy to assign outside the shift, and having 2 separate layers in a PA
Scenario: Assign
	Given I have the role 'Anywhere Team Green'
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-18'
	And I input these assign activity values
	| Field      | Value |
	| Activity   | Phone |
	| Start time | 11:00 |
	| End time   | 12:00 |
	And I click 'apply'
	Then I should see 'Pierre Baldi' with scheduled activity
	| Field      | Value |
	| Start time | 11:00 |
	| End time   | 12:00 |
	| Color      | Green |
	
# If only assigning on shift
Scenario: Assign on shift	
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 11:00 |
	| End time       | 2013-11-19 17:00 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-18'
	And I input these assign activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 13:00 |
	| End time   | 14:00 |
	And I click 'apply'
	Then I should see 'Pierre Baldi' with scheduled activity
	| Field      | Value  |
	| Start time | 13:00  |
	| End time   | 14:00  |
	| Color      | Yellow |

# If only assigning on shift
Scenario: Prevent assign outside shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 11:00 |
	| End time       | 2013-11-19 17:00 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-18'
	And I input these assign activity values
	| Field      | Value |
	| Activity   | Phone |
	| Start time | 17:00 |
	| End time   | 18:00 |
	And I click 'apply'
	Then I should the validation error message 'Cannot assing activity outside shift'

Scenario: Assign after midnight on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 22:00 |
	| End time       | 2013-11-20 04:00 |
	When I view person schedules assign activity form for 'Pierre Baldi' and 'Team green' on '2013-11-19'
	And I input these assign activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 01:00 |
	| End time   | 02:00 |
	And I click 'apply'
	Then I should see 'Pierre Baldi' with scheduled activity
	| Field      | Value            |
	| Start time | 2013-11-20 01:00 |
	| End time   | 2013-11-20 02:00 |
	| Color      | Yellow           |