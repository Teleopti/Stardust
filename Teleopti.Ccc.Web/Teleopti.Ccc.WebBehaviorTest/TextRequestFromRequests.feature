﻿Feature: Text request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as text

Scenario: Add text request
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input text request values
	And I click the OK button
	Then I should see the text request in the list

Scenario: Default text-request values from request view
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	Then I should see the text request form with today's date as default
	And I should see 8:00 - 17:00 as the default times

Scenario: Adding invalid text request values
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input empty subject
	And I input later start time than end time
	And I click the OK button
	Then I should see texts describing my errors
	And I should not see the text request in the list

Scenario: Adding too long text request
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input too long text request values
	And I click the OK button
	Then I should see texts describing too long text error
	And I should not see the text request in the list

Scenario: Adding too long subject request
	Given I am an agent
	And I am viewing requests
	When I click add request button in the toolbar
	And I input too long subject request values
	And I click the OK button
	Then I should see texts describing too long subject error
	And I should not see the text request in the list

Scenario: View text request details
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click on the request
	Then I should see the text request's details form 
	And I should see the request's values
	
Scenario: Edit text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click on the request
	And I input new text request values
	And I click the OK button
	Then I should see the new text request values in the list

Scenario: Delete text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click the request's delete button
	Then I should not see the text request in the list

Scenario: Can not edit approved text requests
	Given I am an agent
	And I have an approved text request
	And I am viewing requests
	When I click on the request
	Then I should see the text request's details form
	And I should not be able to input values
	And I should not see a save button

Scenario: Can not edit denied text requests
	Given I am an agent
	And I have a denied text request
	And I am viewing requests
	When I click on the request
	Then I should see the text request's details form
	And I should not be able to input values
	And I should not see a save button

Scenario: Can not delete approved text request
	Given I am an agent
	And I have an approved text request
	When I am viewing requests
	Then I should not see a delete button

Scenario: Can not delete denied text request
	Given I am an agent
	And I have a denied text request
	When I am viewing requests
	Then I should not see a delete button
