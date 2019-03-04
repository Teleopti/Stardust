@MyTimeAbsence
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

Scenario: Do not show personal account when select an absence type not tracked
Given I am an agent
And I have an open workflow control set with absence request waitlisting enabled
And I view my week schedule for date '2014-10-03'
When I click on the day summary for date '2014-10-03'
And I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should not see the remaining and used time

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
