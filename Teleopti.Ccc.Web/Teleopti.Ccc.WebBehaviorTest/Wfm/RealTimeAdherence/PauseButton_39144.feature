@RTA
@ignore
Feature: Pause button
  In order to have time to analyze current situation
  As a real time analyst
  I want to be able to pause screen updates
  ...So I can be interupted without losing context
  ...So I can take screenshots

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And I have a role with full access
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-05-19 |
	And Pierre Baldi has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-05-20 08:00 |
	  | End time   | 2016-05-20 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	And there is a rule with
	  | Field       | Value     |
	  | Activity    | Phone     |
	  | Phone state | LoggedOut |

  Scenario: Pause
	Given the time is '2016-05-20 08:00:00'
	And 'Pierre Baldi' sets his phone state to 'Ready'
	When I view real time adherence for all agents on team 'Red'
	Then I should see 'Pierre Baldi' in state 'Ready'
	When I pause screen updates
	And 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And I wait 10 seconds
	Then I should see 'Pierre Baldi' in state 'Ready'
