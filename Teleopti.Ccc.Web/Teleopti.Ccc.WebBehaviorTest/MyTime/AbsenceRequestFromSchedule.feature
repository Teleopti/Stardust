﻿@MyTimeAbsence
Feature: Absence request from schedule
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
	| Name        | Holiday |
	| Color       | Red      |
	| Requestable | True     |
	| TrackerType | Day      |
	
	@NotKeyExample
Scenario: Open add absence request form from day summary
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	Then I should see the add absence request form

	@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
Scenario: Add absence request from week schedule view
	Given I am an agent
	And I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	And I input absence request values with 'Holiday' for date '2013-10-03'
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
	And I have an open workflow control set with absence request waitlisting enabled
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	And I click to add a new absence request
	And I input absence request values with 'Vacation' for date '2013-10-03'
	When I click the cancel button
	Then I should not see a symbol at the top of the schedule for date '2013-10-03'

	@NotKeyExample
Scenario: Adding invalid absence request values
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	And I input empty subject
	And I click the send button
	Then I should see subject is missing error
	And I should not see a symbol at the top of the schedule for date '2013-10-03'
	
Scenario: View absence types
	Given I have the role 'Full access to mytime'
	And I have an open workflow control set with absence request waitlisting enabled
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click to add a new absence request
	Then I should see an absence type called Vacation in droplist

@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
Scenario: When requesting absence tracked as days view remaining and used days
Given I am an agent
And I am american
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I have an absence with
| Field     | Value            |
| Name      | Holiday  |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Holiday' for date '2014-10-03'
Then I should see the remaining days is '22 Days'
And I should see the used days is '3 Days'

@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
@NotKeyExample
Scenario: When requesting absence tracked by hours view remaining and used time
Given I am an agent
And there is an absence with
| Field          | Value   |
| Name           | Illness |
| TrackerType    | Time    |
| Requestable    | True    |
| InContractTime | True    |
And I have a personal account with
| Field       | Value      |
| Absence     | Illness    |
| FromDate    | 2014-01-01 |
| Accrued     | 250:00     |
And there is a contract with
| Field                     | Value         |
| Name                      | 8 hours a day |
| Average work time per day | 8:00          |
And I have an absence with
| Field     | Value            |
| Name      | Illness          |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Illness' for date '2014-10-03'
Then I should see the used time is '24:00'
And I should see the remaining time is '226:00'

@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
Scenario: When changing absence type update remaining and used time
Given I am an agent
And I am american
And there is an absence with
| Field            | Value   |
| Name             | Illness |
| TrackerType      | Time    |
| Requestable      | True    |
And I have a personal account with
| Field       | Value      |
| Absence     | Illness    |
| FromDate    | 2014-01-01 |
| Accrued     | 250:00     |
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Illness' for date '2014-10-03'
And I see the remaining time is '250:00'
And I see the used time is '0:00'
And I input absence request values with 'Holiday' for date '2014-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

Scenario: Do not show personal account when select an absence type not tracked
Given I am an agent
And I have an open workflow control set with absence request waitlisting enabled
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should not see the remaining and used time

@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
Scenario: When changing request date change remaining and used time
Given I am an agent
And I am american
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I have an absence with
| Field     | Value            |
| Name      | Holiday  |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2015-01-01      |
| Accrued  | 25              |
And I view my week schedule for date '2014-10-03'
And I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Holiday' for date '2014-10-03'
And I see the remaining time is '22 Days'
And I see the used time is '3 Days'
When I input absence request values with 'Holiday' for date '2015-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

@OnlyRunIfDisabled('MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446')
@NotKeyExample
Scenario: When requesting absence over multiple account periods show remaining and used time according to end date period
Given I am an agent
And I am american
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I have a personal account with
| Field    | Value           |
| Absence  | Holiday |
| FromDate | 2015-01-01      |
| Accrued  | 25              |
And I have an absence with
| Field     | Value            |
| Name      | Holiday  |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-04 23:59 |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with "Holiday" from "2014-12-28" to "2015-10-22"
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

Scenario: Do Not show personal account when you do not have permission
Given there is a role with
| Field                              | Value                                 |
| Name                               | No access to personal absence account |
| Access to personal absence account | False                                 |
And I have the role 'No access to personal absence account'
And I have an open workflow control set with absence request waitlisting enabled
And I have a personal account with
| Field    | Value           |
| Absence  | Vacation |
| FromDate | 2014-01-01      |
| Accrued  | 25              |
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
Then I should not see the remaining and used time
