Feature: Absence request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as absence

@ignore
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
| EndTime   | 2014-01-04 23:59 |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should see the remaining days is '21 Days'
And I should see the used days is '4 Days'

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
And I have an absence with
| Field     | Value            |
| Name      | Illness          |
| StartTime | 2014-01-01 00:00 |
| EndTime   | 2014-01-05 23:59 |
And I am viewing requests
When I click to add a new absence request
And I input absence request values with 'Illness' for date '2014-10-03'
Then I should see the used time is '60:00'
And I should see the remaining time is '190:00'

@ignore
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
And I see the used time is '00:00'
And I input absence request values with 'Vacation' for date '2014-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

@ignore
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
| EndTime   | 2014-01-04 23:59 |
And I have a personal account with
| Field       | Value      |
| Absence     | Vacation   |
| FromDate    | 2015-01-01 |
| Accrued     | 25         |
And I am viewing requests
And I click to add a new absence request
And I input absence request values with 'Vacation' for date '2014-10-03'
And I see the remaining time is '21 days'
And I see the used time is '4 days'
When I input absence request values with 'Vacation' for date '2015-10-03'
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

@ignore
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
And I input absence request values with 'Vacation' for
| Field | Value      |
| From  | 2014-12-28 |
| To    | 2015-01-02 |
Then I should see the remaining days is '25 Days'
And I should see the used days is '0 Days'

@ignore
Scenario: Don't show personal account when you do not have permission
Given I am an agent without permission for personal account
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

Scenario: Add absence request
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Vacation
	And I click the send button
	Then I should see the absence request containing 'Vacation' at position '1' in the list

Scenario: Add absence request when multiple absences exist
	Given I am an agent
	And I have a requestable absence called Vacation
	And I have a requestable absence called Holiday
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Holiday
	And I click the send button
	Then I should see the absence request containing 'Holiday' at position '1' in the list

Scenario: Can not add absence request from request view if no permission
	Given I am an agent without access to absence requests
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
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should see the absence request's values at position '1' in the list

Scenario: Edit absence request
	Given I am an agent
	And I have a requestable absence called Illness
	And I have an existing absence request
	And I am viewing requests
	When I click on the request at position '1' in the list
	And I change the absence request values with
	| Field         | Value          |
	| ListPosistion | 1              |
	| Absence       | Illness        |
	| Subject       | my new subject |
	And I click the update button on the request at position '1' in the list
	Then I should see the updated absence request values in the list with
	| Field         | Value          |
	| ListPosistion | 1              |
	| Absence       | Illness        |
	| Subject       | my new subject |

Scenario: Delete absence request
	Given I am an agent
	And I have an existing absence request
	And I am viewing requests
	When I click the absence request's delete button for request at position '1' in the list
	Then I should not see any requests in the list

Scenario: Can not edit approved absence requests
	Given I am an agent
	And I have an approved absence request
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should not be able to input values for absence request at position '1' in the list
	And I should not see a save button for request at position '1' in the list

Scenario: Can not edit denied absence requests
	Given I am an agent
	And I have a denied absence request
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should not be able to input values for absence request at position '1' in the list
	And I should not see a save button for request at position '1' in the list

Scenario: Can not delete approved absence request
	Given I am an agent
	And I have an approved absence request
	When I am viewing requests
	Then I should not see a delete button for request at position '1' in the list

Scenario: Can not delete denied absence request
	Given I am an agent
	And I have a denied absence request
	When I am viewing requests
	Then I should not see a delete button for request at position '1' in the list
	
Scenario: Can see why absence request was denied
	Given I am an agent
	And I have a denied absence request beacuse of missing workflow control set
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should see that request at position '1' in the list was denied with reason 'Din förfrågan kunde inte behandlas. Du har inget arbetsflöde uppsatt.'