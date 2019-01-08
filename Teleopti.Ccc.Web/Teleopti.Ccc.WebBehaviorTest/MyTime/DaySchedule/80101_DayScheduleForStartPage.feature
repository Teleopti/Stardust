Feature: 80101 - Day Schedule 
	As an agent 
	I want to view dayoff schedule with overtime activities from mobile

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is multiplicator definition set
	| Field | Value                          |
	| Name  | TestMultiplicatorDefinitionSet |
	And there is a contract with
	| Field                        | Value                          |
	| Name                         | A test contract                |
	| Multiplicator definition set | TestMultiplicatorDefinitionSet |
	And There is a skill to monitor called 'Phone' with queue id '9' and queue name 'queue1' and activity 'activity1'
	And there is forecast data for skill 'Phone' opened whole day for next two weeks
	And I am an agent
	And I am american
	And I have a workflow control set with overtime request open periods and auto approval
	And I have the role 'Full access to mytime'
	And I have a person period with
	| Field      | Value           |
	| Start date | 2018-02-01      |
	| Contract   | A test contract |
	| Skill      | Phone           |

@Mobile
Scenario: Show overtime day off schedule
	Given I have a day off scheduled on tomorrow
    When I am viewing mobile view for tomorrow
    And I click the menu button in start page
    And I click menu Overtime Request
    And I fill overtime request form with subject 'my test overtime'
    And I save overtime request
	And I am viewing mobile view for tomorrow
	Then I should see mobile day view