@OnlyRunIfEnabled('MyReport_AgentQueueMetrics_22254')
Feature: My Report Agent Queue Metrics
	In order to improve my own performance
	As an agent
	I need to see metrics for a specific day


Scenario: No permission to my report queue metrics
	Given there is a role with
	| Field                             | Value                                |
	| Name                              | No access to my report queue metrics |
	| Access to my report queue metrics | False                                |
	And I have the role 'No access to my report queue metrics'
	When I view my queue metrics report for '2013-10-02'
	Then I should see a message saying I dont have access

Scenario: Show details for queue metrics
	Given I am an agent
	And I have my report data for '2013-10-02'
	When I view my queue metrics report for '2013-10-02'
	Then I should see the queue metrics report with data for '2013-10-02'

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

Scenario: Show friendly message when no report data
	Given I am an agent
	And I do not have any report data for date '2013-10-02'
	And I view my queue metrics report for '2013-10-02'
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: Navigate from agent queue view to overview
	Given I am an agent
	And I view my queue metrics report for '2013-10-02'
	When I choose to go to overview
	Then I should end up in my report for '2013-10-02'