Feature: My Report Agent Queue Metrics
	In order to improve my own performance
	As an agent
	I need to see metrics for a specific day

@ignore
Scenario: No permission to agent queue metrics from average handling time
	Given there is a role with
	| Field                         | Value                            |
	| Name                          | No access to agent queue metrics |
	| Access to agent queue metrics | False                            |
	And I have the role 'No access to agent queue metrics'
	When I navigate to my report
	And I click on average handling time
	Then I should see a message saying I could not access to detailed agent queue metrics

@ignore
Scenario: No permission to agent queue metrics from average talk time
	Given there is a role with
	| Field                         | Value                            |
	| Name                          | No access to agent queue metrics |
	| Access to agent queue metrics | False                            |
	And I have the role 'No access to agent queue metrics'
	When I navigate to my report
	And I click on average talk time
	Then I should see a message saying I could not access to detailed agent queue metrics

@ignore
Scenario: No permission to agent queue metrics from average after call work
	Given there is a role with
	| Field                         | Value                            |
	| Name                          | No access to agent queue metrics |
	| Access to agent queue metrics | False                            |
	And I have the role 'No access to agent queue metrics'
	When I navigate to my report
	And I click on average after call work
	Then I should see a message saying I could not access to detailed agent queue metrics

@ignore
Scenario: No permission to agent queue metrics from answered calls
	Given there is a role with
	| Field                         | Value                            |
	| Name                          | No access to agent queue metrics |
	| Access to agent queue metrics | False                            |
	And I have the role 'No access to agent queue metrics'
	When I navigate to my report
	And I click on answered calls
	Then I should see a message saying I could not access to detailed agent queue metrics

@ignore
Scenario: Navigate to details from average handling time
	Given I am an agent
	And I view my report for '2014-05-15'
	When I click on average handling time
	Then I should see agent queue view for '2014-05-15'

@ignore
Scenario: Navigate to details from average talk time
	Given I am an agent
	And I view my report for '2014-05-15'
	When I click on average talk time
	Then I should see agent queue view for '2014-05-15'

@ignore
Scenario: Navigate to details from average after call work
	Given I am an agent
	And I view my report for '2014-05-15'
	When I click on average after call work
	Then I should see agent queue view for '2014-05-15'

@ignore
Scenario: Navigate to details from answered calls
	Given I am an agent
	And I view my report for '2014-05-15'
	When I click on answered calls
	Then I should see agent queue view for '2014-05-15'


Scenario: Show details for queue metrics
	Given I am an agent
	And I have my report data for '2013-10-02'
	When I view my queue metrics report for '2013-10-02'
	Then I should see the queue metrics report for '2013-10-02'

@ignore
Scenario: Show details for average talk time for two queues
	Given I am an agent
	And I view my report for '2014-05-15'
	And I have report data for 2 queues
	When I click on average talk time
	Then I should see average talk time for each queue in agent queue view for '2014-05-15'

@ignore
Scenario: Show details for average after call work for two queues
	Given I am an agent
	And I view my report for '2014-05-15'
	And I have report data for 2 queues
	When I click on average after call work
	Then I should see average after call work for each queue in agent queue view for '2014-05-15'

@ignore
Scenario: Show details for answered calls for two queues
	Given I am an agent
	And I view my report for '2014-05-15'
	And I have report data for 2 queues
	When I click on answered calls
	Then I should see answered calls for each queue in agent queue view for '2014-05-15'


Scenario: Navigate within agent queue  to previous day
	Given I am an agent
	And I view my queue metrics report for '2014-05-15'
	When I navigate to the previous day
	Then I should see the queue metrics report for '2014-05-14'
	
Scenario: Navigate within agent queue view to next day
	Given I am an agent
	And I view my queue metrics report for '2014-05-15'
	When I navigate to the next day
	Then I should see the queue metrics report for '2014-05-16'

Scenario: Navigate within agent queue view using date picker
	Given I am an agent
	And I view my queue metrics report for '2014-05-15'
	When I select the date '2014-05-20'
	Then I should see the queue metrics report for '2014-05-20'

@ignore
Scenario: Show friendly message when no report data
	Given I am an agent
	And I do not have any report data for date '2013-10-02'
	When I navigate to agent queue view for '2013-10-02'
	Then I should see a user-friendly message explaining I dont have anything to view

@ignore
Scenario: Navigate from agent queue view to overview
	Given I am an agent
	And I view agent queue metrics for '2014-05-15'
	When I navigate to overview
	Then I should end up in my report for '2014-05-15'

