﻿Feature: Absence request from schedule
	In order to make requests to my superior
	As an agent
	I want to be able to submit absence requests

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field						 | Value						 |
	| Name						 | No access to absence requests |
	| Access to absence requests | False					     |
	And there is an absence with
	| Field       | Value    |
	| Name        | Vacation |
	| Color       | Red      |
	| Requestable | True     |

Scenario: Open add absence request form from day summary
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	Then I should see the add absence request form

Scenario: Add absence request from week schedule view
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	And I input absence request values with 'Vacation' for date '2013-10-03'
	And I click send request button
	Then I should see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: Can not add absence request from day symbol area if no permission
	Given I have the role 'No access to absence requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	Then I should not see the add absence button

Scenario: Can not add absence request from day summary if no permission
	Given I have the role 'No access to absence requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	Then I should not see the add absence button
	
Scenario: Default absence request values from week schedule
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	Then I should see the request form with '2013-10-03' as default date
	And I should see 00:00 - 23:59 as the default times
	
Scenario: Default absence request values from week schedule when unchecked Fullday
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	And I unchecked the full day checkbox
	Then I should see the request form with '2013-10-03' as default date
	And I should see 08:00 - 17:00 as the default times
	
Scenario: Default absence request values from week schedule when checked Fullday
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	And I checked the full day checkbox
	Then I should see the request form with '2013-10-03' as default date
	And I should see 00:00 - 23:59 as the default times
	
Scenario: Cancel adding absence request
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	And I input absence request values with 'Vacation' for date '2013-10-03'
	When I click the cancel button
	Then I should not see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: Adding invalid absence request values
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	And I input empty subject
	And I input later start time than end time for date '2013-10-03'
	And I click send request button
	Then I should see texts describing my errors
	And I should not see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: View absence types
	Given I have the role 'Full access to mytime'
	And I have a requestable absence called Vacation
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	Then I should see an absence type called Vacation in droplist

Scenario: When requesting absence tracked as days view remaining and used days
Given I am an agent
And I am american
And I have a requestable absence with
| Field       | Value           |
| Name        | VacationTracked |
| TrackerType | Day             |
And I have a personal account with
| Field    | Value           |
| Absence  | VacationTracked |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I have an absence with
| Field     | Value            |
| Name      | VacationTracked  |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'VacationTracked' for date '2014-10-03'
Then I should see the remaining days is '22 Days'
And I should see the used days is '3 Days'
