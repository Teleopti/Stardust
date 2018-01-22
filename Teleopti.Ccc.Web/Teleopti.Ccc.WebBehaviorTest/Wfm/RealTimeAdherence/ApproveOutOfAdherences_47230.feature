@RTA
@OnlyRunIfEnabled('RTA_ApprovePreviousOOA_47230')
Feature: Approve out of adherences
  As a Manager I need to approve hours of Out of Adherence for a certain day and agent,
  so that the daily percentage adherence number is increased.

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Hammersmith'
	And there is a team named 'Motorhead' on site 'Hammersmith'
	And I have a role with full access
	And Mikkey Dee has a person period with
	  | Field      | Value      |
	  | Team       | Motorhead  |
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

  Scenario: See recorded out of adherence

  Scenario: See approved periods
  # see approved line
  # see approved table

  Scenario: Approve as in adherence
 # update line
  # update percentage
  # update approved table
  # not update recorded
