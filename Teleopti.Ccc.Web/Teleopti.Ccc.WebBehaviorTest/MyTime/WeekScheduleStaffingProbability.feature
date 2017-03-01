# For function description only
@ignore
@OnlyRunIfEnabled('MyTimeWeb_ViewIntradayStaffingProbability_41608')
Feature: View staffing probability within week schedule
	In order to check if there is probability to get absence / overtime
	As an agent
	I want to see an indication of probability to get absence / overtime

Background: 
Background:
	Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime |
	And There is a skill with
	| Field           | Value                     |
	| Name            | Phone                     |
	| Forecast Method | SkillTypeInboundTelephony |
	And There is valid forecast for skill "Phone"
	And there is a workflow control set with
	| Field          | Value                   |
	| Name           | Intraday Staffing Check |
	| Staffing check | Intraday                |
	And there is a workflow control set with
	| Field          | Value                                  |
	| Name           | Intraday With Shrinkage Staffing Check |
	| Staffing check | Intraday With Shrinkage                |
	And there is a workflow control set with
	| Field          | Value                        |
	| Name           | Budgetgroup head count check |
	| Staffing check | budgetgroup head count       |
	And I has a person period with
	| Field      | Value      |
	| Start date | 2017-01-01 |
	| Skills     | Phone      |

Scenario: Hide staffing probability by default
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday Staffing Check'
	When I view my week schedule for date '2017-02-20'
	Then I should see the probability type "Hide staffing probability" selected
	And I should not see staffing probability

Scenario: Show probability type option for current week only
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday Staffing Check'
	When I view my week schedule for date '2017-02-28'
	Then I should see the probability type "Hide staffing probability" selected
	And I should not see staffing probability
	
Scenario: Hide absence probability option when check staffing not by intraday or by intraday with shrinkage
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup head count check'
	When I view my week schedule for date '2017-02-20'
	Then I should not see the probability type "Absence probability" in probability type selection list

Scenario: Show absence probability option only when check staffing by intraday or by intraday with shrinkage
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set "Intraday Staffing Check" or "Intraday With Shrinkage Staffing Check"
	When I view my week schedule for date '2017-02-20'
	Then I should not see the probability type "Absence probability" in probability type selection list

Scenario: Show absence probability within schedule time range
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday Staffing Check'
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "08:30:00" to "16:30:00" for "2017-02-20"

Scenario: Show no absence probability for day off or full day absence
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set "Intraday Staffing Check"
	And I was scheduled day off or full day absence for '2017-02-20'
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see no absence probability for "2017-02-20"

Scenario: Show correct absence probability for cross day schedule
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set "Intraday Staffing Check"
	And I have schedule from "2017-02-19 18:30:00" to "2017-02-20 02:30:00"
	And I have schedule from "2017-02-20 15:00:00" to "2017-02-20 23:00:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "00:00:00" to "02:30:00" for "2017-02-20"
	And I should see no absence probability from "02:30:00" to "15:30:00" for "2017-02-20"
	And I should see absence probability from "15:00:00" to "23:30:00" for "2017-02-20"

Scenario: Show overtime probability within open hour period
	Given the time is '2017-02-20 20:00'
	And There is site open hour from "10:00" to "15:00" for my site
	And I have the role 'Full access to mytime'
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show overtime probability"
	Then I should see overtime probability from "10:30" to "15:00" for "2017-02-20"

Scenario: Show correct overtime probability for cross day schedule
	Given the time is '2017-02-20 20:00'
	And There is site open hour from "10:00" to "15:00" for my site
	And I have the role 'Full access to mytime'
	And I have schedule from "2017-02-19 18:30:00" to "2017-02-20 02:30:00"
	And I have schedule from "2017-02-20 15:00:00" to "2017-02-20 23:00:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show overtime probability"
	Then I should see overtime probability from "10:30" to "15:00" for "2017-02-20"

Scenario: Show absence probability after current time masked
	Given the time is '2017-02-20 20:00'
	And I have the role 'Full access to mytime'
	And I have the workflow control set "Intraday Staffing Check"
	And I have schedule from "2017-02-20 08:30:00" to "2017-02-20 16:30:00"
	And current time is "2017-02-20 10:08:00"
	When I view my week schedule for date '2017-02-20'
	And I selected "Show absence probability"
	Then I should see absence probability from "08:30:00" to "16:30:00" for "2017-02-20"
	And I should see absence probability from "08:30:00" to "10:00:00" for "2017-02-20" are masked
