@RTA
@OnlyRunIfEnabled('RTA_RemoveAdjustedToNeutral_82147')
Feature: Remove adjusted to neutral period
  In order to handle an incorrectly adjusted to neutral period
  As a Manager
  I want to include time periods into the adherence score calculation by removing a previously adjusted to neutral period
  
  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'London'
	And there is a team named 'Green' on site 'London'
	And I have a role with full access
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2014-01-21 |
	And there is a rule named 'Adhering' with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Adherence   | In    |
	And there is a rule named 'Not adhering' with
	  | Field       | Value     |
	  | Activity    | Phone     |
	  | Phone state | LoggedOff |
	  | Adherence   | Out       |
	
  Scenario: Remove adjusted to neutral period
	Given the period between '2019-03-13 09:00' and '2019-03-13 12:00' is adjusted to neutral
	And the time is '2019-03-14 08:00'
	When I view adjust adherence
	And I remove the adjusted to neutral between '2019-03-13 09:00' and '2019-03-13 12:00'
	Then I should not see the adjusted period between '2019-03-13 09:00' and '2019-03-13 12:00'
