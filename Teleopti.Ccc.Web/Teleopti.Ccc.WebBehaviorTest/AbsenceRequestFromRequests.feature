Feature: Absence request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as absence

Scenario: Add absence request
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Vacation
	And I click the send button
	Then I should see the absence request containing 'Vacation' in the list

Scenario: Add absence request when multiple absences exist
	Given I am an agent
	And I have a requestable absence called Vacation
	And I have a requestable absence called Holiday
	And I am viewing requests
	When I click to add a new absence request
	And I input absence request values with Holiday
	And I click the send button
	Then I should see the absence request containing 'Holiday' in the list

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
	And I should not see the absence request in the list

Scenario: Adding too long message on absence request
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I input too long message request values
	And I click the send button
	Then I should see texts describing too long text error
	And I should not see the absence request in the list

Scenario: Adding too long subject on absence request
	Given I am an agent
	And I am viewing requests
	When I click to add a new absence request
	And I input too long subject request values
	And I click the send button
	Then I should see texts describing too long subject error
	And I should not see the absence request in the list

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
	When I click on the request
	Then I should see the edit absence request form 
	And I should see the absence request's values

Scenario: Edit absence request
	Given I am an agent
	And I have a requestable absence called Illness
	And I have an existing absence request
	And I am viewing requests
	When I click on the request
	And I input new absence request values
	And I click the update button
	Then I should see the new absence request values in the list

Scenario: Delete absence request
	Given I am an agent
	And I have an existing absence request
	And I am viewing requests
	When I click the absence request's delete button
	Then I should not see the absence request in the list

Scenario: Can not edit approved absence requests
	Given I am an agent
	And I have an approved absence request
	And I am viewing requests
	When I click on the request
	Then I should see the edit absence request form
	And I should not be able to input values for absence request
	And I should not see a save button

Scenario: Can not edit denied absence requests
	Given I am an agent
	And I have a denied absence request
	And I am viewing requests
	When I click on the request
	Then I should see the edit absence request form
	And I should not be able to input values for absence request
	And I should not see a save button

Scenario: Can not delete approved absence request
	Given I am an agent
	And I have an approved absence request
	When I am viewing requests
	Then I should not see a delete button

Scenario: Can not delete denied absence request
	Given I am an agent
	And I have a denied absence request
	When I am viewing requests
	Then I should not see a delete button
	
Scenario: Can see why absence request was denied
	Given I am an agent
	And I have a denied absence request beacuse of missing workflow control set
	And I am viewing requests
	When I click on the request
	Then I should see the edit absence request form
	And I should see that my request was denied with reason 'Din förfrågan kunde inte behandlas. Du har inget arbetsflöde uppsatt.'

Scenario: Clear deny reason when adding new absence request
	Given I am an agent
	And I have a denied absence request beacuse of missing workflow control set
	And I am viewing requests
	When I click on the request 
	When I click to add a new absence request
	Then I should not see the deny reason