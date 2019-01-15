@RTA
Feature: Approve out of adherences
  In order to ...
  As a manager
  I want to approve hours of Out of Adherence for a certain day and agent,
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
	Given Mikkey Dee has a 'Phone' shift between '2018-01-22 09:00' and '17:00'
	And today is '2018-01-22'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '10:00' 'Mikkey Dee' sets his phone state to 'Ready'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-22'
	Then I should see recorded out of adherence between '09:00:00' and '10:00:00'

  Scenario: See approved periods
	Given Mikkey Dee has a 'Phone' shift between '2018-01-22 09:00' and '17:00'
	And time is '2018-01-22 17:00'
	And Mikkey Dee has an approved period between '09:00' and '10:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-22'
	Then I should see approved period between '09:00:00' and '10:00:00'

  Scenario: Approve as in adherence
	Given Mikkey Dee has a 'Phone' shift between '2018-01-22 09:00' and '17:00'
	And today is '2018-01-22'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And at '10:00' 'Mikkey Dee' sets his phone state to 'Ready'
	When I view historical adherence for 'Mikkey Dee' on '2018-01-22'
	And I approve out of adherence starting at '09:00:00' as in adherence
	Then I should see approved period between '09:00:00' and '10:00:00'
	And I should not see any out of adherences
	And I should see adherence percentage of 100%
	And I should see recorded out of adherence between '09:00:00' and '10:00:00'
