Feature: Alert agent activity is changing
As an agent 
I need to be alerted before change in activity,
so that I do not forget to switch from Backoffice to Phone at the right time,
so that I can get even better adherence to schedule.

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
	| Day   |
	| Night |
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
	And I have a shift with
	| Field                         | Value            |
	| Shift category                | Night              |
	| Activity                      | Phone            |
	| StartTime                     | 2030-01-02 23:00 |
	| EndTime                       | 2030-01-03 07:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2030-01-03 03:00 |
	| Scheduled activity end time   | 2030-01-03 03:30 |
	And I have a shift with
	| Field                         | Value            |
	| Shift category                | Day            |
	| Activity                      | Phone            |
	| StartTime                     | 2030-01-03 15:00 |
	| EndTime                       | 2030-01-03 23:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2030-01-03 18:00 |
	| Scheduled activity end time   | 2030-01-03 18:30 |
	And I am american

Scenario: Alert agent before first activity starts
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 07:56:59'
	And Alert Time setting is '180' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 07:57:00'
	Then I should see a notify message contains text Phone
	And I should see a notify message contains text coming
	And I should see a notify message contains text 8:00 AM

Scenario: Alert agent before next activity starts
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 10:55:59'
	And Alert Time setting is '240' seconds    
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 10:56:00'
	Then I should see a notify message contains text Lunch
	And I should see a notify message contains text coming
	And I should see a notify message contains text 11:00 AM

Scenario: Alert agent before last activity ends
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 16:58:59'
	And Alert Time setting is '60' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 16:59:00'
	Then I should see a notify message contains text Current shift will end
	And I should see a notify message contains text 5:00 PM

Scenario: Do not alert agent Before Alert Time
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 10:55:00'
	And Alert Time setting is '120' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 10:57:00'
	Then I should not see any notify

Scenario: Do not alert agent After Alert Time 
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 11:45:00'
	And Alert Time setting is '60' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 11:45:01'
	Then I should not see any notify

Scenario: Do not alert agent without permission for ASM
	Given I have the role 'No access to ASM'
	And the current time is '2030-01-01 10:57:59'
	And Alert Time setting is '120' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 10:58:00'
	Then I should not see any notify

Scenario: Automatical close pop up notify message
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 10:57:59'
	And Alert Time setting is '120' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 10:58:00'
	Then I should see a notify message contains text Lunch
	And I should see a notify message contains text coming
	When current browser time has changed to '2030-01-01 10:58:30'
	Then I should not see pop up notify message

Scenario: Should alert agent when now is between 2 shift
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-03 14:56:59'
	And Alert Time setting is '180' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-01 14:57:00'
	Then I should see a notify message contains text Phone
	And I should see a notify message contains text coming
	And I should see a notify message contains text 3:00 PM

Scenario: Should alert agent latest activity when now is at latest activity of 2 nearby shift
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-03 06:56:59'
	And Alert Time setting is '180' seconds                      
	When I am viewing week schedule
	And current browser time has changed to '2030-01-03 06:57:00'
	Then I should see a notify message contains text Current shift will end
	And I should see a notify message contains text 7:00 AM
