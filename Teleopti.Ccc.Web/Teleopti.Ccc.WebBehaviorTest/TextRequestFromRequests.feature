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
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should see the text request's values at position '1' in the list
	
Scenario: Edit text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click on the request at position '1' in the list
	And I change the text request values with
	| Field         | Value          |
	| ListPosistion | 1              |
	| Subject       | my new subject |
	And I click the update button on the request at position '1' in the list
	Then I should see the updated text request values in the list with
	| Field         | Value          |
	| ListPosistion | 1              |
	| Subject       | my new subject |

Scenario: Delete new text request
	Given I am an agent
	And I have an existing text request
	And I am viewing requests
	When I click the delete button of request at position '1' in the list
	Then I should not see any requests in the list

Scenario: Delete pending text request
	Given I am an agent
	And I have a pending text request
	And I am viewing requests
	When I click the delete button of request at position '1' in the list
	Then I should not see any requests in the list

Scenario: Can not edit approved text requests
	Given I am an agent
	And I have an approved text request
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should not be able to input values for text request at position '1' in the list
	And I should not see a save button for request at position '1' in the list

Scenario: Can not edit denied text requests
	Given I am an agent
	And I have a denied text request
	And I am viewing requests
	When I click on the request at position '1' in the list
	Then I should see the detail form for request at position '1' in the list
	And I should not be able to input values for text request at position '1' in the list
	And I should not see a save button for request at position '1' in the list

Scenario: Can not delete approved text request
	Given I am an agent
	And I have an approved text request
	When I am viewing requests
	Then I should not see a delete button for request at position '1' in the list

Scenario: Can not delete denied text request
	Given I am an agent
	And I have a denied text request
	When I am viewing requests
	Then I should not see a delete button for request at position '1' in the list

Scenario: Cancel adding a new text request
	Given I am an agent
	And I am viewing requests
	When I click to add a new text request
	And I click the cancel button
	Then the add request form should be closed