@RTA
Feature: Recent out of adherences
  In order to ??
  As a real time analyst
  I want to see recent out of adherence times

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
	And John King has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2016-05-19 |
	And John King has a shift with
	  | Field      | Value            |
	  | Activity   | Phone            |
	  | Start time | 2016-05-20 08:00 |
	  | End time   | 2016-05-20 17:00 |
	And there is a rule with
	  | Field       | Value |
	  | Activity    | Phone |
	  | Phone state | Ready |
	  | Adherence   | In    |
	And there is a rule with
	  | Field       | Value     |
	  | Activity    | Phone     |
	  | Phone state | LoggedOut |
	  | Adherence   | Out       |

  Scenario: See recent out of adherence
	Given at '2016-05-20 08:00:00' 'Pierre Baldi' sets his phone state to 'Ready'
	And at '2016-05-20 09:10:00' 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And at '2016-05-20 09:20:00' 'Pierre Baldi' sets his phone state to 'Ready'
	And at '2016-05-20 09:45:00' 'Pierre Baldi' sets his phone state to 'LoggedOut'
	And the time is '2016-05-20 10:00:00'
	When I view real time adherence for all agents on team 'Red'
	Then I should see agent status
	  | Field                      | Value        |
	  | Name                       | Pierre Baldi |
	  | Past out of adherence time | 0:10:00      |
	  | Out of adherence time      | 0:15:00      |
