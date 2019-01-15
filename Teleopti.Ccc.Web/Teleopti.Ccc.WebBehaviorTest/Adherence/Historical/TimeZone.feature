@RTA
Feature: Time zone
  In order to improve hawaii agents adherence
  As a real time analyst
  I want to see adherence things

  Background:
	Given there is a switch

  Scenario: See adherence percentage when call center is in Hawaii
	Given I am located in Hawaii
	And 'Pierre Baldi' is located in Hawaii
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2014-10-06 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Start time | 2014-10-06 11:00 |
	  | End time   | 2014-10-06 19:00 |
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
	When the utc time is '2014-10-06 21:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	And the utc time is '2014-10-07 01:00:00'
	And 'Pierre Baldi' sets his phone state to 'Pause'
	And the utc time is '2014-10-07 05:00:00'
	And I view historical adherence for 'Pierre Baldi'
	Then I should see adherence percentage of 50%
