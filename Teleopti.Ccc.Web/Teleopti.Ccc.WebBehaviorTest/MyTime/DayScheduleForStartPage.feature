Feature: Day Schedule For Start Page
	As an agent I need to see the Day schedule as start page in my mobile,
	I need to see the summary info for today's schedule and do some actions for it, such as add absence request, overtime etc.
	I need to see count of requests and unread messages etc. for the day I choose,
	So that I can easlily get information and make a plan just in my mobile.

Background: 
Given there is a role with
	| Field                        | Value                 |
	| Name                         | Full access to mytime |
	| AccessToOvertimeAvailability | true                  |
	
	And there is a shift category with 
	| Field | Value |
	| Name  | Early |
	| Color | Green |
	And there is an absence with
	| Field       | Value    |
	| Name        | Vacation |
	| Color       | Red      |
	| Requestable | true     |
	And there is a workflow control set with
	| Field                        | Value              |
	| Name                         | Published schedule |
	| Schedule published to date   | 2059-02-01         |
	| ReportableAbsence            | Vacation           |
	| ShiftTradeSlidingPeriodStart | 1                  |
	| ShiftTradeSlidingPeriodEnd   | 99                 |
	| AvailableAbsence             | Vacation           |
	| StaffingCheck                | intraday           |

	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
	And I have the role 'Full access to mytime'
	And I am englishspeaking swede
	And I have the workflow control set 'Published schedule'

@Mobile
Scenario: Navigate to day schedule when clicking 'Schedule' menu on mobile
	Given I am american
	When I am viewing requests
	And I click Schedule menu item
	Then I should see mobile day view

Scenario: View day schedule after login
	Given I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-21 09:00 |
	| EndTime        | 2017-04-21 18:00 |
	| Shift category | Early            |
	And I am american
	When I am viewing mobile view for date '2017-04-21'
	Then I should see my day view schedule with
		| Field          | Value             |
		| Date           | 4/21/2017         |
		| Time span      | 9:00 AM - 6:00 PM |
		| Shift category | Early             |
		| Week day       | Friday            |

Scenario: Should see the brief request in today
	Given 'I' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2017-04-21 10:00 |
	| End Time    | 2017-04-21 14:00 |
	| Update Time | 2017-04-21 08:00 |
	| Status      | Pending          |
	When I am viewing mobile view for date '2017-04-21'
	Then I should see the request icon
	When I click the request icon
	Then I should see it go to request page

Scenario: Should see unread messages
	Given I have an unread message with
	| Field | Value       |
	| Title | New message |
	When I am viewing mobile view for date '2017-04-23'
	Then I should see I have '1' unread message(s)
	When I click the message icon
	Then I could see the message with title 'New message'

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could add overtime Availability
When I am viewing mobile view for date '2017-04-21'
When I click the menu button in start page
And I click menu Overtime Availability
And I input '18:00' as overtime startTime and '19:00' as overtime endTime
And I click save request
Then I should see '18:00 - 19:00' Overtime Availability in schedule

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could add absence report
When I am viewing mobile view for today
When I click the menu button in start page
And I click menu menu Absence Reporting
And I click save Absence Report
Then I should see '08:00 - 16:00' 'Vacation' in schedule

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could add absence request
When I am viewing mobile view for date '2017-04-22'
When I click the menu button in start page
And I click menu Absence Request
And I input 'subject 2017-04-22' as subject and 'message 2017-04-22' as message
And I click save request
Then I should see the request icon

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could add text request
When I am viewing mobile view for date '2017-04-23'
When I click the menu button in start page
And I click menu Text Request
And I input 'subject 2017-04-23' as subject and 'message 2017-04-23' as message
And I click save request
Then I should see the request icon

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could redirect to shift trade request
When I am viewing mobile view for date '2017-04-24'
When I click the menu button in start page
And I click menu Shift Trade Request
Then I should see shift trade request page on date '20170424'

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_Command_44209')
Scenario: Could post shift for trade
When I am viewing mobile view for tomorrow
When I click the menu button in start page
And I click menu Post Shift for Trade
And I click save request
Then I should see the request icon
