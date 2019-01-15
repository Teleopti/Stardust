@RTA
Feature: Percentage
  In order to ...
  As a real time analyst
  I want to see ...

  Background:
	Given there is a switch

  Scenario: See adherence percentage from agent state overview
	Given there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2014-10-06 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Start time | 2014-10-06 08:00 |
	  | End time   | 2014-10-06 10:00 |
	  | Activity   | Phone            |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | In    |
	  | Activity    | Phone |
	  | Phone state | Ready |
	And there is a rule with
	  | Field       | Value |
	  | Adherence   | Out   |
	  | Activity    | Phone |
	  | Phone state | Pause |
	When the time is '2014-10-06 08:00:01'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the time is '2014-10-06 09:00:01'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the time is '2014-10-06 11:00:00'
	And I view historical adherence for 'Pierre Baldi'
	Then I should see adherence percentage of 50%
