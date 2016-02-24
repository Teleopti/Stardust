Feature: Absence Report On Desktop Week Schedule
 In order to report absence directly
 As an agent
 I want to report I need be on absence for today or tomorrow directly when I feel sick for example.

 Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field                    | Value                       |
	| Name                     | No access to absence report |
	| Access to absence report | False                       |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2023-06-24         |
	| Reportable absence         | Illness            |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2022-08-19 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2022-08-19 |
	

Scenario: Open add absence report form from day summary only for today and tomorrow
	Given I have the role 'Full access to mytime'
	And the time is '2020-10-05 08:00:00'
	And I view my week schedule for date '2020-10-05'
	When I click on the day summary for date '2020-10-05'
	And I click to add a new absence report
	Then I should see the add absence report form
	When I click on the day summary for date '2020-10-06'
	And I click to add a new absence report
	Then I should see the add absence report form
	When I click on the day summary for date '2020-10-07'
	Then I should not see the add absence report button

Scenario: Cancel a draft absence report
	Given I have the role 'Full access to mytime'
	And the time is '2020-10-05'
	And I view my week schedule for date '2020-10-05'
	When I click on the day summary for date '2020-10-05'
	And I click to add a new absence report
	And I cancel the current absence report draft
	Then I should not see the add absence report form

Scenario: Save an absence report
	Given I have the role 'Full access to mytime'
	And the time is '2020-10-05'
	And I view my week schedule for date '2020-10-05'
	When I click on the day summary for date '2020-10-05'
	And I click to add a new absence report
	And I save the current absence report
	Then I should not see the add absence report form

Scenario: Can not add absence report if no permission
	Given I have the role 'No access to absence report'
	And I view my week schedule for date '2020-10-05'
	When I click on the day symbol area for date '2020-10-05'
	Then I should not see the add absence report button

