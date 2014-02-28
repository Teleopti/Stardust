Feature: View week schedule
	In order to know how to work this week
	As an agent
	I want to see my schedule details
	
Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	Given there is a role with
	| Field                          | Value                   |
	| Name                           | Only access to Anywhere |
	| Access To Anywhere             | true                    |
	| Access To Mytime Web           | false                   |
	| Access To Asm                  | false                   |
	| Access To Text Requests        | false                   |
	| Access To Absence Requests     | false                   |
	| Access To Shift Trade Requests | false                   |
	| Access To Text Requests        | false                   |
	| Access To Extended Preferences | false                   |
	| Access To Preferences          | false                   |
	| Access To Team Schedule        | false                   |
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And there is a workflow control set with
	| Field                                 | Value                            |
	| Name                                  | Published schedule to 2012-08-28 |
	| Schedule published to date            | 2012-08-28                       |
	| Preference period is closed           | true                             |
	| Student availability period is closed | true                             |
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
	| Night |
	| Day   |
	| Late  |
	And there are activities
	| Name           | Color  |
	| Lunch          | Yellow |
	| White activity | White  |
	| Black activity | Black  |

Scenario: No access to schedule page
	Given I have the role 'Only access to Anywhere'
	And I navigate to Anywhere
	And I select application logon data source
	And I sign in
	And I should see Anywhere
	When I manually navigate to week schedule page
	Then I should see an error message

Scenario: View current week
	Given I have the role 'Full access to mytime'
	And the current time is '2030-10-03 12:00'
	When I view my week schedule for date '2030-10-03'
	Then I should see the start and end dates of current week for date '2030-10-03'

Scenario: View night shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                         | Value            |
	| Shift category                | Night            |
	| StartTime                     | 2012-08-27 20:00 |
	| EndTime                       | 2012-08-28 04:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2012-08-27 23:00 |
	| Scheduled activity end time   | 2012-08-28 00:00 |
	When I view my week schedule for date '2012-08-27'
	Then I should not see the end of the shift on date '2012-08-27'
	And I should see the the shift ending at '04:00' on date '2012-08-28'
	
Scenario: Do not show unpublished schedule
	Given I have the role 'Full access to mytime'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-28 8:00  |
	| EndTime               | 2012-08-28 17:00 |
	| Shift category		| Day	           |
	When I view my week schedule for date '2012-08-28'
	Then I should not see any shifts on date '2012-08-28'
	
Scenario: Do not show unpublished schedule for part of week
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule to 2012-08-28'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-28 8:00  |
	| EndTime               | 2012-08-28 17:00 |
	| Shift category		| Day	           |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-29 8:00  |
	| EndTime               | 2012-08-29 17:00 |
	| Shift category		| Day	           |
	When I view my week schedule for date '2012-08-28'
	Then I should see a shift on date '2012-08-28'
	And I should not see a shift on date '2012-08-29'
	
Scenario: View public note
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a public note with
	| Field     | Value      |
	| Date      | 2012-08-28 |
	| Note text | My note    |
	When I view my week schedule for date '2012-08-28'
	Then I should see the public note on date '2012-08-28'

Scenario: Show text request symbol
	Given I have the role 'Full access to mytime'
	And I have an existing text request with
	| Field     | Value            |
	| StartTime | 2013-10-03 10:00 |
	| End Time  | 2013-10-03 14:00 |
	When I view my week schedule for date '2013-10-03'
	Then I should see a symbol at the top of the schedule for date '2013-10-03'

Scenario: Multiple day text requests symbol
	Given I have the role 'Full access to mytime'
	And I have an existing text request with
	| Field     | Value            |
	| StartTime | 2013-10-03 20:00 |
	| End Time  | 2013-10-04 04:00 |
	When I view my week schedule for date '2013-10-03'
	Then I should see a symbol at the top of the schedule for date '2013-10-03'
	And I should not see a symbol at the top of the schedule for date '2013-10-04'

Scenario: Show both text and absence requests
	Given I have the role 'Full access to mytime'
	And I have an existing absence request with
	| Field     | Value            |
	| StartTime | 2013-10-03 10:00 |
	| End Time  | 2013-10-03 14:00 |
	When I view my week schedule for date '2013-10-03'
	Then I should see a symbol at the top of the schedule for date '2013-10-03'

Scenario: Navigate to request page by clicking request symbol
	Given I have the role 'Full access to mytime'
	And I have an existing text request with
	| Field     | Value            |
	| StartTime | 2013-10-03 10:00 |
	| End Time  | 2013-10-03 14:00 |
	When I view my week schedule for date '2013-10-03'
	And I click the request symbol for date '2013-10-03'
	Then I should see request page

Scenario: Navigate to current week
	Given I have the role 'Full access to mytime'
	And the current time is '2030-01-01 07:00'
	And I view my week schedule for date '2029-12-01'
	When I click the current week button
	Then I should see the start and end dates of current week for date '2030-01-01'

Scenario: Show timeline with no schedule
	Given I have the role 'Full access to mytime'
	When I view my week schedule for date '2013-10-03'
	Then I should see start timeline and end timeline according to schedule with:
	| Field						| Value |
	| start timeline			| 0:00  |
	| end timeline				| 23:00 |
	| number of timeline labels	| 24    |

@ignore
Scenario: Show calender according to the users culture
	Given I have the role 'Full access to mytime'
	And I am swedish
	When I view my week schedule for date '2013-10-03'
	And I open the weekschedule date-picker
	Then I should see 'Mo' as the first day in the calender

Scenario: Show timeline with schedule 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-27 10:00 |
	| EndTime               | 2012-08-27 20:00 |
	| Lunch3HoursAfterStart | true             |
	| Shift category		| Late	           |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-28 08:00 |
	| EndTime               | 2012-08-28 17:00 |
	| Lunch3HoursAfterStart | true             |
	| Shift category		| Day	           |
	When I view my week schedule for date '2012-08-27'
	Then I should see start timeline and end timeline according to schedule with:
	| Field						| Value |
	| start timeline			| 8:00  |
	| end timeline				| 20:00 |
	| number of timeline labels	| 13    |

Scenario: Show timeline with night shift
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-27 20:00 |
	| EndTime               | 2012-08-28 04:00 |
	| Lunch3HoursAfterStart | true             |
	| Shift category		| Night	           |
	When I view my week schedule for date '2012-08-27'
	Then I should see start timeline and end timeline according to schedule with:
	| Field						| Value |
	| start timeline			| 0:00  |
	| end timeline				| 23:00 |
	| number of timeline labels	| 24    |

Scenario: Show timeline with night shift from the last day of the previous week
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-26 20:00 |
	| EndTime               | 2012-08-27 04:00 |
	| Lunch3HoursAfterStart | true             |
	| Shift category		| Night	           |
	When I view my week schedule for date '2012-08-27'
	Then I should see start timeline and end timeline according to schedule with:
	| Field						| Value |
	| start timeline			| 0:00  |
	| end timeline				| 4:00	|
	| number of timeline labels	| 5		|

Scenario: Show timeline with night shift starting on the last day of current week
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-26 20:00 |
	| EndTime               | 2012-08-27 04:00 |
	| Lunch3HoursAfterStart | true             |
	| Shift category		| Night	           |
	When I view my week schedule for date '2012-08-26'
	Then I should see start timeline and end timeline according to schedule with:
	| Field						| Value	|
	| start timeline			| 20:00	|
	| end timeline				| 23:00 |
	| number of timeline labels	| 4		|

Scenario: Show activity at correct times
	Given I have the role 'Full access to mytime'
	And I am swedish
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                         | Value            |
	| Shift category                | Day              |
	| StartTime                     | 2012-08-27 08:00 |
	| EndTime                       | 2012-08-27 18:00 |
	| Scheduled activity            | Lunch            |
	| Scheduled activity start time | 2012-08-27 11:00 |
	| Scheduled activity end time   | 2012-08-27 12:00 |
	When I view my week schedule for date '2012-08-27'
	Then I should see activities on date '2012-08-27' with:
	| Field                 | Value         |
	| First activity times  | 08:00 - 11:00 |
	| Second activity times | 11:00 - 12:00 |
	| Third activity times  | 12:00 - 18:00 |

Scenario: Update schedule when schedule has changed
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2012-08-28 12:00 |
	| EndTime               | 2012-08-28 15:00 |
	| Shift category		| Day	           |
	When I view my week schedule for date '2012-08-28'
	And My schedule between '2013-08-28 12:00' to '2013-08-28 15:00' reloads
	Then I should see activities on date '2012-08-28'

@ignore
#Ignored for now. See TextRequestFromSchedule.feature. /Maria S
Scenario: Keep user request input when schedules are refreshed
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I input text request values with subject 'request subject' for date '2013-10-03'
	And My schedule between '2013-10-03 12:00' to '2013-10-03 15:00' reloads
	Then I should see request form with subject 'request subject'

Scenario: Show black day summary text when background color is white 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2013-01-30 08:00 |
	| EndTime               | 2013-01-30 18:00 |
	| Shift category        | Day              |
	| Shift color           | White            |
	When I view my week schedule for date '2013-01-30'
	Then I should see the day summary text for date '2013-01-30' in 'black'

Scenario: Show white day summary text when background color is black 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2013-01-30 08:00 |
	| EndTime               | 2013-01-30 18:00 |
	| Shift category		| Day	           |
	| Shift color           | Black            |
	When I view my week schedule for date '2013-01-30'
	Then I should see the day summary text for date '2013-01-30' in 'white'

Scenario: Show black activity text when activity background color is white
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | White activity   |
	| StartTime      | 2013-01-30 08:00 |
	| EndTime        | 2013-01-30 18:00 |
	When I view my week schedule for date '2013-01-30'
	Then I should see the text for date '2013-01-30' in 'black'

Scenario: Show white activity text when activity background color is black
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a shift with
	| Field          | Value            |
	| Shift category | Day              |
	| Activity       | Black activity   |
	| Start time     | 2013-01-30 08:00 |
	| End time       | 2013-01-30 18:00 |
	When I view my week schedule for date '2013-01-30'
	Then I should see the text for date '2013-01-30' in 'white'

Scenario: Show white absence text when absence background color is black
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a full-day absence today with
	| Field         | Value      |
	| Absence color | Black      |
	| Date          | 2013-01-01 |
	When I view my week schedule for date '2013-01-01'
	Then I should see the text for date '2013-01-01' in 'white'

Scenario: Show black absence text when absence background color is white
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Published schedule'
	And I have a full-day absence today with
	| Field         | Value      |
	| Absence color | White      |
	| Date          | 2013-01-01 |
	When I view my week schedule for date '2013-01-01'
	Then I should see the text for date '2013-01-01' in 'black'


	 
	 