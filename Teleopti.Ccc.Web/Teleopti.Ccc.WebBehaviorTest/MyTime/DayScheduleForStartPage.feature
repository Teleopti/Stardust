Feature: Day Schedule For Start Page
	As an agent I need to see the Day schedule as start page in my mobile,
	I need to see the summary info for today's schedule and do some actions for it, such as add absence request, overtime etc.
	I need to see probability for both absence and overtime for a specific day,
	I need to see count of requests and unread messages etc. for the day I choose,
	So that I can easlily get information and make a plan just in my mobile.

Background: 
Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime | 
	And there is a shift category with 
	| Field | Value |
	| Name  | Early |
	| Color | Green |
	And there is a workflow control set with
	| Field                                 | Value              |
	| Name                                  | Published schedule |
	| Schedule published to date            | 2018-02-01         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-08-19 |

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: View day schedule after login
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
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

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: Should view schedule for tomorrow
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I am american
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-22 09:00 |
	| EndTime        | 2017-04-22 18:00 |
	| Shift category | Early            |
	When I am viewing mobile view for date '2017-04-21'
	And I navigate to next day
	Then I should see my day view schedule with
		| Field          | Value             |
		| Date           | 4/22/2017         |
		| Time span      | 9:00 AM - 6:00 PM |
		| Shift category | Early             |
		| Week day       | Saturday          |
	
@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: Should view schedule for the day before
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I am american
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-22 09:00 |
	| EndTime        | 2017-04-22 18:00 |
	| Shift category | Early            |
	When I am viewing mobile view for date '2017-04-23'
	And I navigate to previous day
	Then I should see my day view schedule with
		| Field          | Value             |
		| Date           | 4/22/2017         |
		| Time span      | 9:00 AM - 6:00 PM |
		| Shift category | Early             |
		| Week day       | Saturday          |

@ignore
@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: Should view today schedule from other day
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I am american
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-21 09:00 |
	| EndTime        | 2017-04-21 18:00 |
	| Shift category | Early            |
	And today is '2017-04-21'
	When I am viewing mobile view for date '2017-04-23'
	And I click today button
	Then I should see my day view schedule with
		| Field          | Value             |
		| Date           | 4/21/2017         |
		| Time span      | 9:00 AM - 6:00 PM |
		| Shift category | Early             |
		| Week day       | Friday            |

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: Should see the brief request in today
	Given I have the role 'Full access to mytime'
	And 'I' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2017-04-21 10:00 |
	| End Time    | 2017-04-21 14:00 |
	| Update Time | 2017-04-21 08:00 |
	| Status      | Pending          |
	When I am viewing mobile view for date '2017-04-21'
	Then I should see the request icon
	When I click the request icon
	Then I should see it go to request page

@OnlyRunIfEnabled('MyTimeWeb_DayScheduleForStartPage_43446')
Scenario: Should see unread messages
	Given I have the role 'Full access to mytime'
	And I have an unread message with
	| Field | Value       |
	| Title | New message |
	When I am viewing mobile view for date '2017-04-23'
	Then I should see I have '1' unread message(s)
	When I click the message icon
	Then I could see the message with title 'New message'

@ignore
Scenario: Should see the absence probability
	Given I have the role 'Full access to mytime'
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to show absence probability
	Then I should see the absnece probability in schedule
	
@ignore
Scenario: Should see the overtime probability
	Given I have the role 'Full access to mytime'
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to show overtime probability
	Then I should see the overtime probability in schedule
	
@ignore
Scenario: Should hide staffing probability
	Given I have the role 'Full access to mytime'
	And I could see absence probability
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to hide staffing probability
	Then I should not see the absence probability in schedule

@ignore
Scenario: Probability setting should be kept when date changed
	Given I have the role 'Full access to mytime'
	And I could see absence probability for date '2017-04-21'
	When I'm viewing at '2017-04-22'
	And change date back to '2017-04-21'
	Then I should see the absence probability in schedule