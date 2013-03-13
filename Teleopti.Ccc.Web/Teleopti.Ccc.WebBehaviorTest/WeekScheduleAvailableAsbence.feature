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
	

Scenario: Do not show indication of the amount of agents that can go on holiday if no permission to absence request
	Given I have the role 'No access to absence requests'
	When I view my week schedule for date '2013-02-15'
	Then I should not see any indication of how many agents can go on holiday

Scenario: Show indication of agents that can go on holiday
	Given I have the role 'Full access to mytime'
	When I view my week schedule for date '2013-02-15'
	Then I should see an indication of the amount of agents that can go on holiday on each day of the week

@ignore
Scenario: Indicate that no agents that can go on holiday if no allowance left
	Given I have the role 'Full access to mytime'
	And There is no available allowance
	When I view my week schedule for date '2013-02-15'
	Then I should see an 'red' indication for chance of absence request on '2013-02-15'

@ignore
Scenario: Show indication of agents that can go on holiday when there is a medium possibility of getting the absence request approved
	Given I have the role 'Full access to mytime'
	And There is 'medium' possibility that I will get an absence request approved for '2013-02-15'
	# Henke: Check with AF: This should probably be changed to something like And the supervisor has granted x of y possible absences
	When I view my week schedule for date '2013-02-15'
	Then I should see an 'yellow' indication for chance of absence request on '2013-02-15'

@ignore	
Scenario: Show indication of agents that can go on holiday when there is no possibility of getting the absence request approved
	Given I have the role 'Full access to mytime'
	And There is 'no' possibility that I will get an absence request approved for '2013-02-15'
	# Henke: Check with AF: This should probably be changed to something like And the supervisor has granted x of y possible absences
	When I view my week schedule for date '2013-02-15'
	Then I should see an 'red' indication for chance of absence request on '2013-02-15'

@ignore
Scenario: Show indication of agents that can go on holiday when there is a high possibility of getting the absence request approved
	Given I have the role 'Full access to mytime'
	And There is 'high' possibility that I will get an absence request approved for '2013-02-15'
	# Henke: Check with AF: This should probably be changed to something like And the supervisor has granted x of y possible absences
	When I view my week schedule for date '2013-02-15'
	Then I should see an 'green' indication for chance of absence request on '2013-02-15'

@ignore
Scenario: Indicate that no agents that can go on holiday if outside bounds of absence period
	Given I have the role 'Full access to mytime'
	And the absence period is opened between '2013-01-01' and '2013-01-30'
	And I have a denied absence request beacuse of missing workflow control set
	When I view my week schedule for date '2013-02-15'
	Then I should see an indication that no agents that can go on holiday for date '2013-02-15'
