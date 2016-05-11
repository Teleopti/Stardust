﻿Feature: Absence request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as absence


Scenario: When requesting absence tracked as days view remaining and used days
Given I am an agent
And I am american
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I have an absence with
| Field     | Value            |
| Name      | Vacation         |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should see the remaining days is '22 Days'
And I should see the used days is '3 Days'

Scenario: When requesting absence tracked by hours view remaining and used time
Given I am an agent
And I have a requestable absence with
| Field       | Value   |
| Name        | Illness |
| TrackerType | Time    |
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
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Illness' for date '2014-10-03'
Then I should see the used time is '24:00'
And I should see the remaining time is '226:00'

Scenario: When changing absence type update remaining and used time
Given I am an agent
And I am american
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a requestable absence with
| Field       | Value   |
| Name        | Illness |
| TrackerType | Time    |
And I have a personal account with
| Field       | Value      |
| Absence     | Illness    |
| FromDate    | 2014-01-01 |
| Accrued     | 250:00     |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Illness' for date '2014-10-03'
And I see the remaining time is '250:00'
And I see the used time is '0:00'
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

Scenario: Do not show personal account when select an absence type not tracked
Given I am an agent
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should not see the remaining and used time

Scenario: When changing request date change remaining and used time
Given I am an agent
And I am american
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I have an absence with
| Field     | Value            |
| Name      | Vacation         |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-03 23:59 |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2015-01-01 |
| Accrued     | 25         |
And I am viewing requests
And I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
And I see the remaining time is '22 Days'
And I see the used time is '3 Days'
When I input absence request values with 'Vacation' for date '2015-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

Scenario: When requesting absence over multiple account periods show remaining and used time according to end date period
Given I am an agent
And I am american
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2015-01-01 |
| Accrued     | 25         |
And I have an absence with
| Field     | Value            |
| Name      | Vacation         |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-04 23:59 |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with "Vacation" from "2014-12-28" to "2015-01-02"
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

Scenario: Do Not show personal account when you do not have permission
Given there is a role with
	| Field                              | Value                                 |
	| Name                               | No access to personal absence account |
	| Access to personal absence account | False                                 |
And I have the role 'No access to personal absence account'
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I am viewing requests
When I click to add a new absence request
Then I should not see the remaining and used time

Scenario: Do not show personal account when absence requests is not editable
Given I am an agent
And I have a requestable absence with
| Field       | Value    |
| Name        | Vacation |
| TrackerType | Day      |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2014-01-01 |
| Accrued     | 25         |
And I have an approved absence request
And I am viewing requests
When I click on the existing request in the list
Then I should not see the remaining and used time

Scenario: Add absence request
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Vacation
	And I click the send button
	Then I should see the request of type 'Vacation' in the list

Scenario: Add absence request when multiple absences exist
	Given I am an agent
	And I have a requestable absence called Vacation
	And I have a requestable absence called Holiday
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Holiday
	And I click the send button
	Then I should see the request of type 'Holiday' in the list

Scenario: Can not add absence request from request view if no permission
	Given I have a role with
         | Field                      | Value |
         | Access To Absence Requests | false |
	When I am viewing requests
	Then I should not see the New Absence Request menu item

Scenario: Default absence request values from request view
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click to add a new absence request
	Then I should see the request form with today's date as default
	And I should see 00:00 - 23:59 as the default times
	And I should see an absence type called Vacation in droplist

Scenario: Default absence request values from request view when checked Fullday
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I checked the full day checkbox
	Then I should see the request form with today's date as default
	And I should see 00:00 - 23:59 as the default times

Scenario: Default absence request values from request view when unchecked Fullday
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I unchecked the full day checkbox
	Then I should see the request form with today's date as default
	And I should see 08:00 - 17:00 as the default times

Scenario: Adding invalid absence request values
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I input empty subject
	And I input later start time than end time
	And I click the send button
	Then I should see texts describing my errors
	And I should not see any requests in the list

Scenario: Adding too long message on absence request
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I try to input too long message request values
	Then I should see message adjusted to maximum length
	
Scenario: Adding too long subject on absence request
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I input too long subject request values
	And I click the send button
	Then I should see texts describing too long subject error
	And I should not see any requests in the list

Scenario: View absence types
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click to add a new absence request
	Then I should see an absence type called Vacation in droplist

Scenario: View absence request details
	Given I am an agent
	And I have an existing absence request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should see the values of the absence request

Scenario: View absence request waitlist
	Given I am an agent
	And I have an open workflow control set with absence request waitlisting enabled
	And I have an auto denied absence request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should see the waitlist position is 1

Scenario: Edit absence request
	Given I am an agent
	And I have a requestable absence called Illness
	And I have an existing absence request
	And I am viewing requests
	When I click on the existing request in the list
	And I change the absence request values with
	| Field         | Value          |
	| Absence       | Illness        |
	| Subject       | my new subject |
	And I submit my changes for the existing text request
	Then I should see the updated values for the existing absence request in the list with
	| Field         | Value          |
	| Absence       | Illness        |
	| Subject       | my new subject |

Scenario: Delete absence request
	Given I am an agent
	And I have an existing absence request
	And I am viewing requests
	When I delete the existing request in the list
	Then I should not see any requests in the list

Scenario: Delete waitlisted absence request
	Given I am an agent
	And I have an open workflow control set with absence request waitlisting enabled
	And I have an auto denied absence request
	And I am viewing requests
	When I delete the existing request in the list
	Then I should not see any requests in the list

Scenario: Can not edit approved absence requests
	Given I am an agent
	And I have an approved absence request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should not be able to edit the values for the existing absence request
	And I should not be able to submit possible changes for the existing request

Scenario: Can not edit denied absence requests
	Given I am an agent
	And I have a denied absence request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should not be able to edit the values for the existing absence request
	And I should not be able to submit possible changes for the existing request

Scenario: Can not edit waitlisted absence requests
	Given I am an agent
	And I have an open workflow control set with absence request waitlisting enabled
	And I have an auto denied absence request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should not be able to edit the values for the existing absence request
	And I should not be able to submit possible changes for the existing request

@ignore
Scenario: Can not delete approved absence request
	Given I am an agent
	And I have an approved absence request
	When I am viewing requests
	Then I should not be able to delete the existing request in the list

Scenario: Can not delete denied absence request
	Given I am an agent
	And I have a denied absence request
	When I am viewing requests
	Then I should not be able to delete the existing request in the list
	
Scenario: Can see why absence request was denied
	Given I am an agent
	And I have a denied absence request beacuse of missing workflow control set
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should see that the request was denied with reason 'Din förfrågan kunde inte behandlas. Du har inget arbetsflöde uppsatt.'
