Feature: ASM
	In order to improve adherence
	As an agent
	I want to see my current activities

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field         | Value            |
	| Name          | No access to ASM |
	| Access To Asm | False            |
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
	And there are shift categories
	| Name  |
	| Day  |
	And there is an activity with
	 | Field | Value |
	 | Name  | Phone |
	 | Color | Green |
	And there is an activity with
	 | Field | Value  |
	 | Name  | Lunch  |
	 | Color | Yellow |
	And I have a shift with
	| Field                         | Value            |
	| Shift category                | Day              |
	| Activity                      | Phone            |
	| StartTime                     | 2030-01-01 08:00 |
	| EndTime                       | 2030-01-01 17:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2030-01-01 11:00 |
	| Scheduled activity end time   | 2030-01-01 12:00 |

Scenario: No permission to ASM module
	Given I have the role 'No access to ASM'
	When I am viewing week schedule
	Then ASM link should not be visible 

Scenario: Show part of agent's schedule in popup
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01'	
	When I view ASM
	Then I should see a schedule in popup

Scenario: Show title in popup
	Given I have the role 'Full access to mytime'
	When I view ASM
	Then I should see a popup with title AgentScheduleMessenger 

Scenario: Current activity should be shown
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 16:00'
	When I view ASM
	Then I should see Phone as current activity

Scenario: No current activity to show
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 07:00'
	When I view ASM
	Then I should not see a current activity

Scenario: Current activity changes
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 11:59'
	When I view ASM
	And current browser time has changed to '2030-01-01 12:00'
	Then I should see Phone as current activity

Scenario: Upcoming activity time period should be displayed
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 00:01'
	When I view ASM
	Then I should see next activity time as '08:00-11:00'

Scenario: Upcoming activity time period starting after midnight should be indicated as next day
	Given I have the role 'Full access to mytime'
	And the current time is '2029-12-31 23:59'
	When I view ASM
	Then I should see next activity time as '08:00+1-11:00'

Scenario: Agent should from ASM popup be notified when current shift has changed
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 00:00'
	When I view ASM
	And My schedule between '2030-01-01 08:00' to '2030-01-01 17:00' change
	Then I should see one notify message

Scenario: Agent should from portal be notified when current shift has changed
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 00:00'
	When I am viewing week schedule
	And My schedule between '2030-01-01 08:00' to '2030-01-01 17:00' change
	Then I should see one notify message

Scenario: Asm should be automatically reloaded when time passes
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 23:59'
	When I view ASM
	Then Now indicator should be at hour '47'
	When current browser time has changed to '2030-01-02 00:01'
	Then Now indicator should be at hour '24'

Scenario: Asm should not indicate unread messages if no messages
	Given I have the role 'Full access to mytime'
	When I view ASM
	Then I shoud not see an indication that I have an unread message

Scenario: Asm should indicate unread messages
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value        |
	| Title         | New message	 |
	When I view ASM
	Then I shoud see an indication that I have '1' unread messages

Scenario: Asm should indicate number of unread messages
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field         | Value        |
	| Title         | New message	 |
	And I have an unread message with
	| Field         | Value					|
	| Title         | Another Message	|
	When I view ASM
	Then I shoud see an indication that I have '2' unread messages

