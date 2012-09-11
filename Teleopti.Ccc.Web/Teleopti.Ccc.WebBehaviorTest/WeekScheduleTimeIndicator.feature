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
	| Access To Agent Schedule Messenger	| false				|
    
Scenario: Do not show time indicator if no permission
	Given I have the role 'No access to ASM'
	And Current time is '2012-10-03 12:00'
	When I view my week schedule for date '2012-10-03'
	Then I should not see the time indicator for date '2012-10-03'

Scenario: Show the time indicator at correct time
	Given I have the role 'Full access to mytime'
	And Current time is '2012-10-03 12:00'
	When I view my week schedule for date '2012-10-03'
	Then I should see the time indicator at time '2012-10-03 12:00'

Scenario: Show time indicator movement
	Given I have the role 'Full access to mytime'
	And Current time is '2012-09-13 12:00'
	And I view my week schedule
	And I should see the time indicator at time '2012-09-13 12:00'
	When Current browser time has changed to '2012-09-13 12:30'
	And I navigate to week schedule page for date '2012-09-13'
	Then I should see the time indicator at time '2012-09-13 12:30'
		
Scenario: Show time indicator movement at midnight
	Given I have the role 'Full access to mytime'
	And Current time is '2012-09-13 23:59'
	And I view my week schedule
	And I should see the time indicator at time '2012-09-13 23:59'
	When Current browser time has changed to '2012-09-14 00:00'
	And I navigate to week schedule page for date '2012-09-14'
	Then I should see the time indicator at time '2012-09-14 00:00'

Scenario: Handle time indicator movement from winter to summer time
	Given I have the role 'Full access to mytime'
	And Current time is '2013-10-27 2:59'
	And I should see the time indicator at time '2013-10-27 2:59'
	When Time has passed with '1' minutes
	Then I should see the time indicator at time '2013-10-28 02:00'

Scenario: Handle time indicator movement from summer to winter time
	Given I have the role 'Full access to mytime'
	And Current time is '2014-03-30 1:59'
	And I should see the time indicator at time '2014-03-30 1:59'
	When Time has passed with '1' minutes
	Then I should see the time indicator at time '2014-03-30 03:00'

Scenario: Do not show time indicator when viewing other week than current
	Given I have the role 'Full access to mytime'
	And Current time is '2014-03-12 12:00'
	When I view my week schedule for date '2014-03-05'
	Then I should not see the time indicator
