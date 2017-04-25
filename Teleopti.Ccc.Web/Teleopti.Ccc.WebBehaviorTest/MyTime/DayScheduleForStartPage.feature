@ignore
Feature: Day Schedule For Start Page
	As an agent I need to see the Day schedule as start page in my mobile,
	I need to see the summary info for today's schedule and do some actions for it, such as add absence request, overtime etc.
	I need to see probability for both absence and overtime for a specific day,
	I need to see count of requests and unread messages etc. for the day I choose,
	So that I can easlily get information and make a plan just in my mobile.


Scenario: View day schedule after login
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-21 09:00 |
	| EndTime        | 2017-04-21 18:00 |
	| Shift category | Early            |
	And today is '2017-04-21'
	When I login my time
	Then I should see my schedule for date '2017-04-21'
	And I should see "Friday(Early 9:00-18:00)" on top of my schedule

Scenario: Should view schedule for tomorrow
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-22 09:00 |
	| EndTime        | 2017-04-22 18:00 |
	| Shift category | Early            |
	When I'm viewing at '2017-04-21'
	And I slip left on my mobile
	Then I should see my schedule for date '2017-04-22'
	
Scenario: Should view schedule for the day before
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-22 09:00 |
	| EndTime        | 2017-04-22 18:00 |
	| Shift category | Early            |
	When I'm viewing at '2017-04-23'
	And I slip right on my mobile
	Then I should see my schedule for date '2017-04-22'

Scenario: Should view schedule for day choose in calandar
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-22 09:00 |
	| EndTime        | 2017-04-22 18:00 |
	| Shift category | Early            |
	When I'm viewing at '2017-04-23'
	And I chosse date '2017-04-22' in calandar
	Then I should see my schedule for date '2017-04-22'

Scenario: Should view today schedule from other day
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field          | Value            |
	| StartTime      | 2017-04-21 09:00 |
	| EndTime        | 2017-04-21 18:00 |
	| Shift category | Early            |
	And today is '2017-04-21'
	When I'm viewing at '2017-04-23'
	And I click today on the top of the view
	Then I should see my schedule for date '2017-04-21'

Scenario: Should see the count of requests in today
	Given I have the role 'Full access to mytime'
	And today is '2017-04-21'
	And I have an intraday absence request
	When I'm viewing at '2017-04-21'
	Then I should see the number of Count of requests is '1'
	When I click the count of request icon
	Then I should see the request page

Scenario: Should see unread messages
	Given I have the role 'Full access to mytime'
	And I have 2 unread messages
	When I login my time
	Then I should see I have '2' unread messages
	When I click the message icon
	Then I could see the message page

Scenario: Should see the absence probability
	Given I have the role 'Full access to mytime'
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to show absence probability
	Then I should see the absnece probability in schedule
	
Scenario: Should see the overtime probability
	Given I have the role 'Full access to mytime'
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to show overtime probability
	Then I should see the overtime probability in schedule
	
Scenario: Should hide staffing probability
	Given I have the role 'Full access to mytime'
	And I could see absence probability
	When I'm viewing at '2017-04-21'
	And I click show probability button and choose to hide staffing probability
	Then I should not see the absence probability in schedule

Scenario: Probability setting should be kept when date changed
	Given I have the role 'Full access to mytime'
	And I could see absence probability for date '2017-04-21'
	When I'm viewing at '2017-04-22'
	And change date back to '2017-04-21'
	Then I should see the absence probability in schedule


Scenario: Could add overtime Avaiability
Scenario: Could add absence report
Scenario: Could add absence request
Scenario: Could add Text request
Scenario: Could add shift trade request
Scenario: Could post shift for trade

Scenario: Should go to team schedule view from current day view
Scenario: Should go to availablity view from current day view
Scenario: Should go to preference view from current day view
Scenario: Should go to request view from current day view
Scenario: Should go to message view from current day view
Scenario: Should go to reports view from current day view
Scenario: Should go to personal setting view from current day view
Scenario: Should go to see badge from current day view