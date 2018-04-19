@RTA
@OnlyRunIfEnabled('RTA_ApprovalPermission_74898')

Feature: Approve out of adherences if permitted
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

  Scenario: Can't approve as in adherence without permission
	Given I have a role with
	  | Field                         | Value       |
	  | Name                          | Team leader |
	  | Access to team                | Motorhead   |
	  | Access to real time adherence | True        |
	  | Modify Adherence              | False       |
	And Mikkey Dee has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And today is '2018-04-19'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '16:30' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And the time is '2018-04-19 17:15'
	When I view historical adherence for 'Mikkey Dee' on '2018-04-19'
	Then I should not see adherence approval action

  Scenario: Can't remove approved adherence without permission
	Given I have a role with
	  | Field                         | Value       |
	  | Name                          | Team leader |
	  | Access to team                | Motorhead   |
	  | Access to real time adherence | True        |
	  | Modify Adherence              | False       |
	And Mikkey Dee has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And today is '2018-04-19'
	And at '09:00' 'Mikkey Dee' sets his phone state to 'Ready'
	And at '16:30' 'Mikkey Dee' sets his phone state to 'LoggedOff'
	And the time is '2018-04-19 17:15'
	And Mikkey Dee has approved out of adherence between '16:30:00' and '17:00:00'
	When I view historical adherence for 'Mikkey Dee' on '2018-04-19'
	Then I should not see adherence removal action
