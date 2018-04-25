@RTA
@OnlyRunIfEnabled('RTA_RestrictModifyAdherenceWithPermission_74898')

Feature: Restrict modify adherence with permission
  As a Manager I need to define who is permitted to approve, and remove approval,
  so that all team leads can approve for their team members, but not for any others
  so that I can remove their approvals where I see fit for all agents.

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Hammersmith'
	And there is a team named 'Motorhead' on site 'Hammersmith'
	And Mikkey Dee has a person period with
	  | Field      | Value      |
	  | Team       | Motorhead  |
	  | Start Date | 2018-04-19 |
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
	And I have a role with
	  | Field                      | Value |
	  | Access to modify adherence | False |

  Scenario: Approve as in adherence restricted by permission
	And Mikkey Dee has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And today is '2018-04-19'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '16:30' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And the time is '17:15'
	When I view historical adherence for 'Mikkey Dee' on '2018-04-19'
	Then I should not be able to approve out of adherences
	
  Scenario: Remove approved out of adherences restricted by permission
	And Mikkey Dee has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And today is '2018-04-19'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '16:30' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And the time is '17:15'
	And Mikkey Dee has an approved period between '16:30' and '17:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-04-19'
	Then I should not be able to remove approved out of adherences
