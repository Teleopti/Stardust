@RTA
Feature: Restrict modify adherence with permission
  In order to ...
  As a manager
  I want to define who is permitted to approve, and remove approval,
  so that all team leads can approve for their team members, but not for any others
  so that I can remove their approvals where I see fit for all agents.

  Background:
	Given there is a switch
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'Green' on site 'Paris'
	And there is a team named 'Red' on site 'Paris'
	And Ashley Andeen has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2018-04-19 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Red        |
	  | Start Date | 2018-04-19 |
	And Pierre Baldi has a person period with
	  | Field      | Value      |
	  | Team       | Green      |
	  | Start Date | 2018-04-20 |
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
	And Ashley Andeen has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And Pierre Baldi has a 'Phone' shift between '2018-04-19 09:00' and '17:00'
	And Ashley Andeen has a 'Phone' shift between '2018-04-20 09:00' and '17:00'
	And Pierre Baldi has a 'Phone' shift between '2018-04-20 09:00' and '17:00'
	And today is '2018-04-19'
	And at '09:00' 'Ashley Andeen' sets his phone state to 'Ready'
	And at '09:00' 'Pierre Baldi' sets his phone state to 'Ready'
	And at '16:30' 'Ashley Andeen' sets his phone state to 'LoggedOff'
	And at '16:30' 'Pierre Baldi' sets his phone state to 'LoggedOff'
	And today is '2018-04-20'
	And at '09:00' 'Ashley Andeen' sets his phone state to 'Ready'
	And at '09:00' 'Pierre Baldi' sets his phone state to 'Ready'
	And at '16:30' 'Ashley Andeen' sets his phone state to 'LoggedOff'
	And at '16:30' 'Pierre Baldi' sets his phone state to 'LoggedOff'
	And the time is '17:15'

  Scenario: Approve as in adherence restricted by access permission
	Given I have a role with
	  | Field                      | Value |
	  | Access to modify adherence | False |
	When I view historical adherence for 'Ashley Andeen' on '2018-04-19'
	Then I should not be able to approve out of adherences

  Scenario: Approve as in adherence restricted by person access
	Given I have a role with
	  | Field                      | Value |
	  | Access to team             | Green |
	  | Access to modify adherence | True  |
	When I view historical adherence for 'Ashley Andeen' on '2018-04-19'
	Then I should be able to approve out of adherences
	When I view historical adherence for 'Pierre Baldi' on '2018-04-19'
	Then I should not be able to approve out of adherences

  Scenario: Approve as in adherence restricted by date access
	Given I have a role with
	  | Field                      | Value |
	  | Access to team             | Green |
	  | Access to modify adherence | True  |
	When I view historical adherence for 'Pierre Baldi' on '2018-04-19'
	Then I should not be able to approve out of adherences
	When I view historical adherence for 'Pierre Baldi' on '2018-04-20'
	Then I should be able to approve out of adherences

  Scenario: Remove approved out of adherences restricted by access permission
	Given I have a role with
	  | Field                      | Value |
	  | Access to modify adherence | False |
	And Ashley Andeen has an approved period between '16:30' and '17:00'
	When I view historical adherence for 'Ashley Andeen' on '2018-04-20'
	Then I should not be able to remove approved out of adherences

  @ignore
 #intent is clear, no need to test
  Scenario: Remove approved out of adherences restricted by person access

  @ignore
 #intent is clear, no need to test
  Scenario: Remove approved out of adherences restricted by date access
