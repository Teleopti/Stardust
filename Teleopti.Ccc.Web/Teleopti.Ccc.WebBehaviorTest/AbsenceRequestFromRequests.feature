Feature: Absence request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as absence

Scenario: Add absence request
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click add request button in the toolbar
	And I click absence request tab
	And I input absence request values with Vacation
	And I click the OK button
	Then I should see the absence request in the list

Scenario: Can not add absence request from request view if no permission
	Given I am an agent without access to absence requests
	And I am viewing requests
	When I click add request button in the toolbar
	Then I should not see the absence request tab

Scenario: Default absence-request values from request view
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click add request button in the toolbar
	And I click absence request tab
	Then I should see the absence request form with today's date as default
	And I should see 8:00 - 17:00 as the default times
	And I should see an absence type called Vacation in droplist

Scenario: Default absence request values from request view When checked Fullday
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I click absence request tab
	And I click full day checkbox
	Then I should see the request form with tomorrow as default date
	And I should see 00:00 - 23:59 as the default times

Scenario: Cancel adding absence request from request view
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click add request button in the toolbar
	And I click absence request tab
	And I input absence request values with Vacation
	And I click the Cancel button
	Then I should see the absence request in the list

Scenario: Adding invalid absence request values
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input empty subject
	And I input later start time than end time
	And I click the OK button
	Then I should see texts describing my errors
	And I should not see the absence request in the list

Scenario: Adding too long message on absence request
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input too long message request values
	And I click the OK button
	Then I should see texts describing too long text error
	And I should not see the absence request in the list

Scenario: Adding too long subject on absence request
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input too long subject request values
	And I click the OK button
	Then I should see texts describing too long subject error
	And I should not see the absence request in the list

Scenario: View absence types
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing requests
	When I click add request button in the toolbar
	And I click absence request tab
	Then I should see an absence type called Vacation in droplist