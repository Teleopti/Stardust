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
  	  
  Scenario: See period out of adherence
  	Given Mikkey Dee has a 'Phone' shift between '2018-01-18 09:00' and '17:00'
	And at '2018-01-18 10:00:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '2018-01-18 11:00:00' 'Mikkey Dee' sets his phone state to 'Ready'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-18'
	Then I should see period out of adherence between '10:00:00' and '11:00:00'
  
  Scenario: See out of adherence line

  Scenario: Approve period out of adherence

  Scenario: Update adherence percentage with approved as in adherence
  
  Scenario: See approved period
	
  Scenario: See approved as in adherence line
	
  Scenario: Not display line for approved as in adherence