Feature: OvertimeRequests
	In order to earn more
	As an agent
	I want to be able to create an overtime request

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field                       | Value                          |
	| Name                        | No access to overtime requests |
	| Access to overtime requests | False                          |
	And there is multiplicator definition set
	| Field | Value                          |
	| Name  | TestMultiplicatorDefinitionSet |
	And there is a contract with
	| Field                        | Value                          |
	| Name                         | A test contract                |
	| Multiplicator definition set | TestMultiplicatorDefinitionSet |
	And I am an agent
	And I am englishspeaking swede
	And There is a skill to monitor called 'Phone' with queue id '9' and queue name 'queue1' and activity 'activity1'
	And there is queue statistics for the skill 'Phone' up until '19:00'
	And there is forecast data for skill 'Phone' for next two weeks
	And I have a person period with
	| Field      | Value           |
	| Start date | 2018-02-01      |
	| Contract   | A test contract |
	| Skill      | Phone           |


Scenario: Add a new overtime request
	Given I have the role 'Full access to mytime'
	And I view my next week schedule
	When I click on the day summary for the first day of next week
	And I open add new overtime request form
	And I fill overtime request form with subject 'my test overtime'
	And I save overtime request
	And I navigate to the requests page
	Then I should see my existing overtime request with subject 'my test overtime'
	Then I should see my existing overtime request with status 'Denied'

Scenario: Edit a pending overtime request
	Given I have created an overtime request with subject 'test overtime request'
	When I am viewing requests
	When I click on the existing request in the list
	And I fill overtime request form with subject 'my new overtime request'
	And I save overtime request
	Then I should see my existing overtime request with subject 'my new overtime request'

Scenario: Add a pending overtime request from schedule
	Given I have the role 'Full access to mytime'
	And I have a workflow control set with overtime request open periods
	And I view my next week schedule
	When I click on the day summary for the first day of next week
	And I open add new overtime request form
	And I fill overtime request form with subject 'my test overtime'
	And I save overtime request
	And I navigate to the requests page
	Then I should see my existing overtime request with subject 'my test overtime'	
	Then I should see my existing overtime request with status 'Pending'

Scenario: Add a pending overtime request from requests
	Given I have the role 'Full access to mytime'
	And I have a workflow control set with overtime request open periods
	When I am viewing requests
	And I open add new overtime request form
	And I fill overtime request form with subject 'my test overtime'
	And I save overtime request
	Then I should see my existing overtime request with subject 'my test overtime'	
	Then I should see my existing overtime request with status 'Pending'