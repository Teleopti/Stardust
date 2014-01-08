Feature: Add activity
	In order to assign work to a working agent
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

Scenario: View form with shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view schedules for 'Team green' on '2013-11-18'
	And I select any schedule activity for 'Pierre Baldi'
	And I click 'add activity' in shift menu
	Then I should see the add activity form

Scenario: View form without shift
	Given I have the role 'Anywhere Team Green'
	When I view schedules for 'Team green' on '2013-11-18'
	And I click person name 'Pierre Baldi'
	And I click 'add activity' in schedule menu
	Then I should see the add activity form

Scenario: View team mates schedules
	Given I have the role 'Anywhere Team Green'
	And 'John King' has a person period with
	| Field      | Value      |
	| Team       | Team green |
	| Start date | 2013-11-18 |
	And 'John King' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-18'
	Then I should see schedule for 'John King'

@ignore
Scenario: Add after midnight on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 22:00 |
	| End time       | 2013-11-20 04:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-19'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 01:00 |
	| End time   | 02:00 |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value            |
	| Start time | 2013-11-20 01:00 |
	| End time   | 2013-11-20 02:00 |
	| Color      | Yellow           |

@ignore
Scenario: Add over midnight on night shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 22:00 |
	| End time       | 2013-11-20 04:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-19'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 23:00 |
	| End time   | 01:00 |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value            |
	| Start time | 2013-11-19 23:00 |
	| End time   | 2013-11-20 01:00 |
	| Color      | Yellow           |
@ignore
Scenario: Prevent creation of second shift
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 11:00 |
	| End time       | 2013-11-19 17:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-19'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Phone |
	| Start time | 17:00 |
	| End time   | 18:00 |
	Then I should see the alert 'Cannot Create Second Shift When Adding Activity'
@ignore
# THE SCENARIOS BELOW ARE APPLICABLE ONLY IF WE CAN ADD ACTIVITIES TO EMPTY DAYS OUT OF THE BOX
Scenario: Default times
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-18 13:20'
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-18'
	Then I should see the add activity form with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 14:30 |

Scenario: Add to day with no shift
	Given I have the role 'Anywhere Team Green'
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-18'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Phone |
	| Start time | 11:00 |
	| End time   | 12:00 |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value |
	| Start time | 11:00 |
	| End time   | 12:00 |
	| Color      | Green |
@ignore
# THE SCENARIOS BELOW ARE APPLICABLE IF WE DECIDE TO ONLY ADD ACTIVITIES TO EXISTING SHIFTS INSTEAD
Scenario: Default times (2)
	Given I have the role 'Anywhere Team Green'
	And the current time is '2013-11-18 13:20'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-18 11:00 |
	| End time       | 2013-11-18 17:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-18'
	Then I should see the add activity form with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 14:30 |

Scenario: Add on shift	
	Given I have the role 'Anywhere Team Green'
	And 'Pierre Baldi' has a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Phone            |
	| Start time     | 2013-11-19 11:00 |
	| End time       | 2013-11-19 17:00 |
	When I view person schedules add activity form for 'Pierre Baldi' in 'Team green' on '2013-11-19'
	And I input these add activity values
	| Field      | Value |
	| Activity   | Lunch |
	| Start time | 13:00 |
	| End time   | 14:00 |
	And I initiate 'apply'
	Then I should see 'Pierre Baldi' with the scheduled activity
	| Field      | Value  |
	| Start time | 13:00  |
	| End time   | 14:00  |
	| Color      | Yellow |

