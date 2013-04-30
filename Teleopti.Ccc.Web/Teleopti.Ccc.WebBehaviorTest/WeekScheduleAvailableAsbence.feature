Feature: View available absences
	In order to pick a good day for absence
	As an agent
	I want to see an indication of possibilities to get a day off

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a role with
	| Field						 | Value						 |
	| Name						 | No access to absence requests |
	| Access to absence requests | False					     |
	And there is an absence with
	| Field                    | Value   |
	| Name                     | holiday |
	And there is a budgetgroup with
	| Field		| Value   |
	| Name		| TheBudgetGroup |
	| Absence   | holiday |
	And there is a workflow control set with
	| Field                      | Value					|
	| Name                       | Open absence period		|
	| Schedule published to date | 2040-06-24				|
	| Available absence          | holiday					|
	And there is a workflow control set with
	| Field                      | Value					|
	| Name                       | Closed absence period	|
	| Schedule published to date | 2040-06-24				|
	| Available absence          | holiday					|
	| Absence period is closed	 | true						|
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| BudgetGroup| TheBudgetGroup |


Scenario: Show the user a green indication when allowance exceeds used absence
	Given there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2013-04-01			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'green' indication for chance of absence request on '2013-04-01'


Scenario: Show the user a yellow indication when there is a fair amount of allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 8						|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'yellow' indication for chance of absence request on '2013-04-01'


Scenario: Show the user a red indication when there is only a little or no allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 15					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'red' indication for chance of absence request on '2013-04-01'

Scenario: Show the user a red indication when there is no budgetgroup for that day
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2013-04-02           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2013-04-05           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-05			|
	| Hours			| 0						|
	| Absence		| holiday				|
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-04-03 |
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2013-04-05'
	Then I should see an 'green' indication for chance of absence request on '2013-04-02'


Scenario: Show the user a red indication when current date is outside open absence periods
	Given there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2013-04-01			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I have the workflow control set 'Closed absence period'
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'red' indication for chance of absence request on '2013-04-01'


Scenario: Do not show indication of the amount of agents that can go on holiday if no permission to absence request
	Given I have the role 'No access to absence requests'
	When I view my week schedule for date '2013-02-15'
	Then I should not see any indication of how many agents can go on holiday


