Feature: Text request from schedule
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as text

Scenario: Add text request from week schedule view
	Given I am an agent
	And I am viewing week schedule
	When I click add text request button in the toolbar
	And I input text request values
	And I click the OK button
	Then I should see a symbol at the top of the schedule
	#And I should see the text request on the request tab

Scenario: Can not add text request if no permission
	Given I am an agent without access to text requests
	When I am viewing week schedule
	Then I should not see the add text request button

Scenario: Default text request values from week schedule
	Given I am an agent
	And I am viewing week schedule
	When I click add text request button in the toolbar
	Then I should see the text request form with the first day of week as default
	And I should see 8:00 - 17:00 as the default times

Scenario: Cancel adding text request
	Given I am an agent
	And I am viewing week schedule
	When I click add text request button in the toolbar
	And I input text request values
	And I click the Cancel button
	Then I should not see a symbol at the top of the schedule

Scenario: Adding invalid text request values
	Given I am an agent
	And I am viewing week schedule
	When I click add text request button in the toolbar
	And I input empty subject
	And I input later start time than end time
	And I click the OK button
	Then I should see texts describing my errors
	And I should not see a symbol at the top of the schedule