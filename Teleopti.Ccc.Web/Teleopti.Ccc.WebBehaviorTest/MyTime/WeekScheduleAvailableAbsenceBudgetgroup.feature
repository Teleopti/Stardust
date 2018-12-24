Feature: View available absences with budgetgroup check
	In order to pick a good day for absence
	As an agent
	I want to see an indication of possibilities to get an absence

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
	| StaffingCheck				 | budgetgroup				|
	#Ska tas bort! /MariaS
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Budgetgroup head count check	|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup head count		|
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

#for bug 42261
Scenario: Traffic light language should be set accordingly
	Given there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-04-01			|
	| Allowance					| 3 					|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I am chinese
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an hint '缺勤申请被批准的机会是：很有可能' for chance of absence request on '2023-04-01'
	And I view my settings
	And I change language to english
	And I view my week schedule for date '2023-04-01'
	And I should see an hint 'Chance of getting absence request granted: Good' for chance of absence request on '2023-04-01'

Scenario: Show the user an empty indication when today is outside open absence periods
	Given the time is '2023-05-02 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-04-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field                         | Value                 |
	| Name                          | Closed absence period |
	| Schedule published to date    | 2040-06-24            |
	| Available absence             | holiday               |
	| AbsenceRequestOpenPeriodStart | 2023-04-10            |
	| AbsenceRequestOpenPeriodEnd   | 2023-04-30            |
	| StaffingCheck					| budgetgroup			|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Closed absence period'
	When I view my week schedule for date '2023-04-28'
	Then I should see no indication for chance of absence request on '2023-04-28'

Scenario: Show the user an empty indication when absence day is outside absence preference periods
	Given the time is '2023-05-15 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field									| Value						|
	| Name									| Closed preference period	|
	| Schedule published to date			| 2040-06-24				|
	| Available absence						| holiday					|
	| AbsenceRequestPreferencePeriodStart	| 2023-05-10				|
	| AbsenceRequestPreferencePeriodEnd		| 2023-05-25				|
	| StaffingCheck							| budgetgroup				|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Closed preference period'
	When I view my week schedule for date '2023-05-28'
	Then I should see no indication for chance of absence request on '2023-05-28'

Scenario: Do not show the user any indication when there is no staffing check for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| No staffing check				|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'No staffing check'
	When I view my week schedule for date '2023-05-28'
	Then I should not see any indication of how many agents can go on holiday

@NotKeyExample
Scenario: Do not show indication of the amount of agents that can go on holiday if no permission to absence request
	Given I have the role 'No access to absence requests'
	When I view my week schedule for date '2023-02-15'
	Then I should not see any indication of how many agents can go on holiday

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication when allowance exceeds used absence
	Given there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-04-01			|
	| Allowance					| 3 					|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'green' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a yellow indication when there is a fair amount of allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 8						|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'yellow' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a red indication when there is only a little or no allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 15					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'red' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user an empty indication when there is no budgetgroup for that day
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-03           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-06           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-06			|
	| Hours			| 0						|
	| Absence		| holiday				|
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2023-04-04 |
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-05'
	Then I should see an 'green' indication for chance of absence request on '2023-04-03'
	And I should see no indication for chance of absence request on '2023-04-04'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Do not show the user any indication when there is intraday staffing check for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value						|
	| Name							| Intraday staffing check	|
	| Schedule published to date	| 2040-06-24				|
	| Available absence				| holiday					|
	| StaffingCheck					| intraday					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday staffing check'
	When I view my week schedule for date '2023-05-28'
	Then I should not see any indication of how many agents can go on holiday
	
@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication when there is budgetgroup check for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Budgetgroup staffing check	|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup staffing check'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'green' indication for chance of absence request on '2023-05-28'
	
@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication when there is no auto grant for absence requests for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| No auto grant					|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'No auto grant'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'green' indication for chance of absence request on '2023-05-28'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication when absence requests are auto granted for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Auto grant					|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	| Auto grant					| yes							|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Auto grant'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'green' indication for chance of absence request on '2023-05-28'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user an empty indication when absence requests are auto denied for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Auto deny						|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	| Auto grant					| deny							|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Auto deny'
	When I view my week schedule for date '2023-05-28'
	Then I should see no indication for chance of absence request on '2023-05-28'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a red indication when left absence is less than one fulltime equivalent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 41					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'red' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication when left absence is more than two fulltime equivalents and more than 30 percent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 31					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'green' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a yellow indication when left absence is more than two fulltime equivalents and less than 30 percent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 20                   |
	| FulltimeEquivalentHours	| 5                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 80					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'yellow' indication for chance of absence request on '2023-04-01'

@OnlyRunIfDisabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a yellow indication when left absence is more than one fulltime equivalent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 35					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'yellow' indication for chance of absence request on '2023-04-01'

# The following tests are replicates with a new 'traffic light' icon
# Please delete the above tests after toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is set to true for a period of time

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication of new icon when allowance exceeds used absence
	Given there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-04-01			|
	| Allowance					| 3 					|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a yellow indication of new icon when there is a fair amount of allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 8						|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'fair' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a red indication of new icon when there is only a little or no allowance compared to used absence
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 15					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'poor' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user an empty indication of new icon when there is no budgetgroup for that day
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-03           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-06           |
	| Allowance					| 2                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-06			|
	| Hours			| 0						|
	| Absence		| holiday				|
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2023-04-04 |
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-05'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-04-03'
	And I should see no indication of new icon for chance of absence request on '2023-04-04'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Do not show the user any indication of new icon when there is intraday staffing check for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value						|
	| Name							| Intraday staffing check	|
	| Schedule published to date	| 2040-06-24				|
	| Available absence				| holiday					|
	| StaffingCheck					| intraday					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Intraday staffing check'
	When I view my week schedule for date '2023-05-28'
	Then I should not see any indication of how many agents can go on holiday

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')	
Scenario: Show the user a green indication of new icon when there is budgetgroup check for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Budgetgroup staffing check	|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Budgetgroup staffing check'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-05-28'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')	
Scenario: Show the user a green indication of new icon when there is no auto grant for absence requests for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| No auto grant					|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'No auto grant'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-05-28'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication of new icon when absence requests are auto granted for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Auto grant					|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	| Auto grant					| yes							|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Auto grant'
	When I view my week schedule for date '2023-05-28'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-05-28'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user an empty indication of new icon when absence requests are auto denied for that day
	Given the time is '2023-05-25 20:00'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2023-05-28			|
	| Allowance					| 2						|
	| FulltimeEquivalentHours	| 8						|
	And there is a workflow control set with
	| Field							| Value							|
	| Name							| Auto deny						|
	| Schedule published to date	| 2040-06-24					|
	| Available absence				| holiday						|
	| StaffingCheck					| budgetgroup					|
	| Auto grant					| deny							|
	And I have the role 'Full access to mytime'
	And I have the workflow control set 'Auto deny'
	When I view my week schedule for date '2023-05-28'
	Then I should see no indication of new icon for chance of absence request on '2023-05-28'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a red indication of new icon when left absence is less than one fulltime equivalent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 41					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'poor' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a green indication of new icon when left absence is more than two fulltime equivalents and more than 30 percent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 31					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'good' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a yellow indication of new icon when left absence is more than two fulltime equivalents and less than 30 percent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 20                   |
	| FulltimeEquivalentHours	| 5                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 80					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'fair' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')	
Scenario: Show the user a yellow indication of new icon when left absence is more than one fulltime equivalent
	Given there is a budgetday
	| Field						| Value                |
	| BudgetGroup				| TheBudgetGroup	   |
	| Date						| 2023-04-01           |
	| Allowance					| 6                    |
	| FulltimeEquivalentHours	| 8                    |
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2023-04-01			|
	| Hours			| 35					|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2023-04-01'
	Then I should see an 'fair' indication of new icon for chance of absence request on '2023-04-01'

@OnlyRunIfEnabled('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640')
Scenario: Show the user a red indication of new icon when allowance exceeds used absence but the date has passed
	Given the time is '2030-01-01'
	And there is a budgetday
	| Field						| Value					|
	| BudgetGroup				| TheBudgetGroup		|
	| Date						| 2013-04-01			|
	| Allowance					| 3 					|
	| FulltimeEquivalentHours	| 8						|
	And I have the role 'Full access to mytime'
	And I have absence time for
	| Field			| Value					|
	| Date			| 2013-04-01			|
	| Hours			| 3						|
	| BudgetGroup	| NameOfTheBudgetGroup	|
	| Absence		| holiday				|
	And I have the workflow control set 'Open absence period'
	When I view my week schedule for date '2013-04-01'
	Then I should see an 'poor' indication of new icon for chance of absence request on '2013-04-01'