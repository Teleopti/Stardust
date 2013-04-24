Feature: View available absences
	In order to pick a good day for absence
	As an agent
	I want to see an indication of possibilities to get a day off

Background:
	Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
	And there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And there is a workflow control set with
	| Field                                 | Value                            |
	| Name                                  | Published schedule to 2012-08-28 |
	| Schedule published to date            | 2012-08-28                       |
	| Preference period is closed           | true                             |
	| Student availability period is closed | true                             |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	
Scenario: Henkes testscenario for setting up budgetgroups
	Given there is an absence with
	| Field                    | Value   |
	| Name                     | holiday |
	And there is a budgetgroup with
	| Field		| Value   |
	| Name		| NameOfTheBudgetGroup |
	| Absence   | holiday |
	And there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| NameOfTheBudgetGroup |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 8						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	When I view my week schedule for date '2013-04-01'
	Then I should see an indication of the amount of agents that can go on holiday on each day of the week


Scenario: Henkes testscenario for setting up budgetgroups 1
	Given there is an absence with
	| Field	| Value   |
	| Name	| holiday |
	And there is a budgetgroup with
	| Field		| Value   |
	| Name		| NameOfTheBudgetGroup |
	| Absence   | holiday |
	And there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| NameOfTheBudgetGroup |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'green' indication for chance of absence request on '2013-04-01'


Scenario: Henkes testscenario for setting up budgetgroups 2
	Given there is an absence with
	| Field	| Value   |
	| Name	| holiday |
	And there is a budgetgroup with
	| Field		| Value   |
	| Name		| NameOfTheBudgetGroup |
	| Absence   | holiday |
	And there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| NameOfTheBudgetGroup |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 8						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'yellow' indication for chance of absence request on '2013-04-01'


Scenario: Henkes testscenario for setting up budgetgroups 3
	Given there is an absence with
	| Field	| Value   |
	| Name	| holiday |
	And there is a budgetgroup with
	| Field		| Value   |
	| Name		| NameOfTheBudgetGroup |
	| Absence   | holiday |
	And there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| NameOfTheBudgetGroup |
	| Date						| 2013-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And there is absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 15					|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'red' indication for chance of absence request on '2013-04-01'

@ignore
Scenario: Do not show indication of the amount of agents that can go on holiday if no permission to absence request
	Given I have the role 'No access to absence requests'
	When I view my week schedule for date '2013-02-15'
	Then I should not see any indication of how many agents can go on holiday

Scenario: Show indication of agents that can go on holiday
	Given I have the role 'Full access to mytime'
	When I view my week schedule for date '2013-02-15'
	Then I should see an indication of the amount of agents that can go on holiday on each day of the week

@ignore
Scenario: Indicate that no agents that can go on holiday if outside bounds of absence period
	Given I have the role 'Full access to mytime'
	And the absence period is opened between '2013-01-01' and '2013-01-30'
	And I have a denied absence request beacuse of missing workflow control set
	When I view my week schedule for date '2013-02-15'
	Then I should see an indication that no agents that can go on holiday for date '2013-02-15'
