Feature: Time indicator in week schedule
	In order to get better control of my weekly schedule 
	As an agent
	I want to see an indication of the current and passed time in the week schedule

Background: 
	Given there is a role with
	| Field                    | Value				   |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field									| Value				|
	| Name									| No access to ASM	|
	| Access to Agent Schedule Messenger	| false				|
    
Scenario: Do not show time indicator if no permission
	Given I have the role 'No access to ASM'
	And Current time is '2001-01-01 12:00'
	When I view my week schedule for date '2001-01-01'
	Then I should not see the time indicator

Scenario: Show the end of time indicator at correct time
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-01 12:00'
	When I view my week schedule for date '2001-01-01'
	Then I should see the end of time indicator at time '2001-01-01 12:00'

Scenario: Do not show time indicator when I view a future week
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-01 12:00'
	When I view my week schedule for date '2001-01-08'
	Then I should not see the time indicator

Scenario: Show time indicator when I view a passed week
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-01 12:00'
	When I view my week schedule for date '2000-12-20'
	Then I should see the time indicator over the whole week

Scenario: Show time indicator for the part of the week that has passed
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-03 12:00'
	When I view my week schedule for date '2001-01-03'
	Then I should see the time indicator from the start of the week up until the time '2001-01-03 12:00'

Scenario: Show time indicator movement
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-03 12:00'
	And I should see the end of time indicator at time '2001-01-03 12:00'
	When Time has passed with '1' minutes 
	Then I should see the end of time indicator at time '2001-01-03 12:01'

Scenario: Show time indicator movement at midnight
	Given I have the role 'Full access to mytime'
	And Current time is '2001-01-01 23:59'
	And I should see the end of time indicator at time '2001-01-01 23:59'
	When Time has passed with '1' minutes
	Then I should see the end of time indicator at time '2001-01-02 00:00'



	

