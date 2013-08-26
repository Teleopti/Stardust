Feature: Text request from schedule
	In order to make requests to my superior
	As an agent
	I want to be able to submit requests as text
	
Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field                      | Value                      |
	| Name                       | No access to text requests |
	| Access to text requests    | False                      |
	| Access to absence requests | True                       |

Scenario: Open add text request form
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	Then I should see the add text request form
	
Scenario: Open add text request form from day summary
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	Then I should see the add text request form
	
Scenario: Add text request from week schedule view
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I input text request values for date '2013-10-03'
	And I click send request button
	Then I should see a symbol at the top of the schedule for date '2013-10-03'

Scenario: Can not add text request from day symbol area if no permission
	Given I have the role 'No access to text requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	Then I should not see the add text request form

Scenario: Can not add text request from day summary if no permission
	Given I have the role 'No access to text requests'
	And I view my week schedule for date '2013-10-03'
	When I click on the day summary for date '2013-10-03'
	Then I should not see the add text request form

Scenario: Cancel adding a new text request from week schedule view
	Given I have the role 'Full access to mytime'
	And I view my week schedule for date '2013-10-03'
	When I click on the day symbol area for date '2013-10-03'
	And I click the cancel button
	Then the add request form should be closed
	And I should not see a symbol at the top of the schedule for date '2013-10-03'