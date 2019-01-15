@RTA
Feature: Rta tool
  In order to manually verify the rta works
  As a tester
  I want to see that the agent view updates when I send in states

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with
	  | Field                | Value |
	  | Access to everything | true  |
	  | Access to team       | Red   |

	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2017-01-01 |
	And there is a state with name 'Ready'

  Scenario: See agent update in agents view
	Given the time is '2017-05-22 08:00'
	And I am viewing the rta tool
	When I send 'Ready' for all agents
	And I view real time adherence for all agents on team 'Red'
	Then I should see agent 'Pierre Baldi' with state 'Ready'