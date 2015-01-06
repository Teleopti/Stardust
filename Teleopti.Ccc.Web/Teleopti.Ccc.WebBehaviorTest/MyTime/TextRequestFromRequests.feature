Feature: Text request from requests
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as text

Scenario: Add text request
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I input text request values
	And I click send request button
	Then I should see the text request in the list

Scenario: Adding invalid text request values
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I input empty subject
	And I input later start time than end time
	And I click send request button
	Then I should see texts describing my errors
	And I should not see any requests in the list

Scenario: Adding too long text request
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I input too long text request values
	Then I should see message adjusted to maximum length

Scenario: Adding too long subject request
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I input too long subject request values
	And I click send request button
	Then I should see texts describing too long subject error
	And I should not see any requests in the list

Scenario: View text request details
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should see the values of the existing text request
	
Scenario: Edit text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click on the existing request in the list
	And I change the subject to 'my new subject' for the existing request
	And I submit my changes for the existing text request
	Then I should see the existing text request in the list with subject 'my new subject'

Scenario: Delete new text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I delete the existing request in the list
	Then I should not see any requests in the list

Scenario: Delete pending text request
	Given I am an agent
	And I have a pending text request
	And I am viewing requests
	When I delete the existing request in the list
	Then I should not see any requests in the list

Scenario: Can not edit approved text requests
	Given I am an agent
	And I have an approved text request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should not be able to edit the values for the existing text request
	And I should not be able to submit possible changes for the existing request

Scenario: Can not edit denied text requests
	Given I am an agent
	And I have a denied text request
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see the detail form for the existing request in the list
	And I should not be able to edit the values for the existing text request
	And I should not be able to submit possible changes for the existing request

Scenario: Can not delete approved text request
	Given I am an agent
	And I have an approved text request
	When I am viewing requests
	Then I should not be able to delete the existing request in the list

Scenario: Can not delete denied text request
	Given I am an agent
	And I have a denied text request
	When I am viewing requests
	Then I should not be able to delete the existing request in the list

Scenario: Cancel adding a new text request
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I click the cancel button
	Then the add request form should be closed