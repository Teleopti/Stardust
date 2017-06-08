Feature: View available absences with head count check
	In order to pick a good day for absence
	As an agent
	I want to see an indication of possibilities to get a day off

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is an absence with
	| Field				| Value		|
	| Name				| holiday	|
	| Requestable		| true		|
	And there is a budgetgroup with
	| Field		| Value			|
	| Name		| TheBudgetGroup|
	| Absence   | holiday		|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Open absence period			|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| Staffing check				| budgetgroup					|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Budgetgroup head count check	|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| Staffing check				| budgetgroup head count		|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Mixed staffing check			|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| Staffing check				| mix							|
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And 'Ashley Andeen' has a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And 'Pierre Baldi' has a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| BudgetGroup| TheBudgetGroup |
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| BudgetGroup| TheBudgetGroup |
	And Pierre Baldi has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| BudgetGroup| TheBudgetGroup |

Scenario: Show the user a green indication at head count staffing check when allowance exceeds used absence
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 3						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup head count check'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'green' indication for chance of absence request on '2023-05-28'

Scenario: Show the user a yellow indication at head count staffing check when one allowance is left
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 3						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup head count check'
	And Ashley Andeen has the workflow control set 'Budgetgroup head count check'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	And Ashley Andeen has absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'yellow' indication for chance of absence request on '2023-05-28'

Scenario: Show the user a red indication at head count staffing check when the allowance is used up
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 3						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup head count check'
	And Ashley Andeen has the workflow control set 'Budgetgroup head count check'
	And Pierre Baldi has the workflow control set 'Budgetgroup head count check'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	And Ashley Andeen has absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	And Pierre Baldi has absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'red' indication for chance of absence request on '2023-05-28'
	
Scenario: Show the user no indication at mixed staffing check when last open period is not validate with budget group
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 3						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Mixed staffing check'
	And Ashley Andeen has the workflow control set 'Mixed staffing check'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	And Ashley Andeen has absence time for
	| Field			| Value					|
	| Date			| 2023-05-28			|
	| Hours			| 1						|
	| Absence		| holiday				|
	When I view my week schedule for date '2023-05-28'
	Then I should see no indication for chance of absence request on '2023-05-28'