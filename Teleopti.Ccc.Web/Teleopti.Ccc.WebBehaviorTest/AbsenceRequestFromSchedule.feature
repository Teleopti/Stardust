Feature: Absence request from schedule
	In order to make requests to my superior
	As an agent
	I want to be able to submit absence requests

Scenario: Add absence request from week schedule view
	Given I am an agent
	And I am viewing week schedule
	When I click on today's summary
	And I click absence request tab
	And I input absence request values
	And I click the OK button
	Then I should see a symbol at the top of the schedule

Scenario: Can not add absence request if no permission
	Given I am an agent without access to absence requests
	And I am viewing week schedule
	When I click on today's summary
	Then I should not see the absence request tab

Scenario: Default absence request values from week schedule
	Given I am an agent
	And I am viewing week schedule
	When I click on tomorrows summary
	And I click absence request tab
	Then I should see the text request form with tomorrow as default date
	And I should see 00:00 - 23:59 as the default times

Scenario: Cancel adding absence request
	Given I am an agent
	And I am viewing week schedule
	When I click on today's summary
	And I click absence request tab
	And I input absence request values
	And I click the Cancel button
	Then I should not see a symbol at the top of the schedule

Scenario: Adding invalid absence request values
	Given I am an agent
	And I am viewing week schedule
	When I click on today's summary
	And I click absence request tab
	And I input empty subject
	And I input later start time than end time
	And I click the OK button
	Then I should see texts describing my errors
	And I should not see a symbol at the top of the schedule

Scenario: View absence types
	Given I am an agent
	And I have a requestable absence called Vacation
	And I am viewing week schedule
	When I click on today's summary
	And I click absence request tab
	Then I should see a absence type called Vacation in droplist
