Feature: Planning Period
	As a resource planner
	I want to work on planning periods

Scenario: show the next planning period
	Given the time is '2014-04-10'
	And I have a role with
		| Field                                 | Value             |
		| Name                                  | Resource Planner  |
		| Access to resource planner            | True              |
	When I view Resource planner
	Then I should see planning period from '2014-05-01'to '2014-05-01'

Scenario: schedule the next planning period
	Given the time is '2014-04-10'
	And I have a role with
		| Field									| Value            |
        | Name									| Resource Planner |
		| Access to resource planner			| True             | 
	When I view Resource planner
	And  I click schedule
	Then I should see '0' are days scheduled
