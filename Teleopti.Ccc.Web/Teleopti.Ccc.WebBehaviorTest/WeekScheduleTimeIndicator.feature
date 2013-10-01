Feature: Week schedule time indicator
	In order to get better control of my weekly schedule 
	As an agent
	I want to see an indication of the current and passed time in the week schedule

Background: 
	Given there is a role with
	| Field                    | Value				   |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field			| Value            |
	| Name			| No access to ASM |
	| Access To Asm	| false            |
	And there are shift categories
	| Name  |
	| Day   |
    
Scenario: Do not show time indicator if no permission
	Given I have the role 'No access to ASM'
	And the current time is '2030-10-03 12:00'
	When I view my week schedule for date '2030-10-03'
	Then I should not see the time indicator for date '2030-10-03'

Scenario: Show the time indicator at correct time
	Given I have the role 'Full access to mytime'
	And the current time is '2030-10-03 12:00'
	When I view my week schedule for date '2030-10-03'
	Then I should see the time indicator at time '2030-10-03 12:00'

Scenario: Show time indicator movement
	Given I have the role 'Full access to mytime'
	And the current time is '2030-03-12 11:00'
	And I view my week schedule for date '2030-03-12'
	And I should see the time indicator at time '2030-03-12 11:00'
	When current browser time has changed to '2030-03-12 11:01'
	Then I should see the time indicator at time '2030-03-12 11:01'
		
Scenario: Show time indicator movement at midnight
	Given I have the role 'Full access to mytime'
	And the current time is '2030-09-20 23:59'
	And I view my week schedule for date '2030-09-20'
	And I should see the time indicator at time '2030-09-20 23:59'
	When current browser time has changed to '2030-09-21 0:00'
	Then I should see the time indicator at time '2030-09-21 0:00'

Scenario: Do not show time indicator when viewing other week than current
	Given I have the role 'Full access to mytime'
	And the current time is '2030-03-12 12:00'
	When I view my week schedule for date '2030-03-05'
	Then I should not see the time indicator

Scenario: Show the time indicator at correct time with a shift
	Given I have the role 'Full access to mytime'
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 10:00 |
	| EndTime               | 2030-01-01 12:00 |
	| Shift category		| Day	           |
	And the current time is '2030-01-01 11:00'
	When I view my week schedule for date '2030-01-01'
	Then I should see the time indicator at time '2030-01-01 11:00'

Scenario: Do not show the time indicator after passing end of timeline
	Given I have the role 'Full access to mytime'
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-03-12 04:00 |
	| EndTime               | 2030-03-12 12:00 |
	| Shift category		| Day	           |
	And the current time is '2030-03-12 12:15'
	And I view my week schedule for date '2030-03-12'
	And I should see the time indicator at time '2030-03-12 12:15'
	When current browser time has changed to '2030-03-12 12:16'
	Then I should not see the time indicator

	
