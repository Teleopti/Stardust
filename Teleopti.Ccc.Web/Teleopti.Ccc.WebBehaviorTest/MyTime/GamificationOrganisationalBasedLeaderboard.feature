@ignore
@OnlyRunIfEnabled('MyTimeWeb_OrganisationalBasedLeaderboard_31184')
Feature: GamificationOrganisationalBasedLeaderboard
	The leaderboard can be based on site/team/everyone
	As an agent
	I want to view the leaderboard based on my data hierarchy

Background: 
	Given there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is a team named 'Team red' on 'The site'
	And Pierre Baldi has a person period with
		| Field      | Value      |
		| Team       | Team red   |
		| Start Date | 2014-01-06 |
	And I have a person period with
		| Field      | Value      |
		| Team       | Team green |
		| Start Date | 2014-01-06 |
	And  There is an agent badge settings with
		| Field                 | Value |
		| BadgeEnabled          | True  |
		| AnsweredCallsUsed     | True  |
		| AHTUsed               | True  |
		| AdherenceUsed         | True  |
		| Silver to bronze rate | 5     |
		| Gold to silver rate   | 2     |
	And I have badges with
		| Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
		| AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
		| AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
		| Adherence           | 3      | 0      | 3    | 2014-08-11         |
	And Pierre Baldi has badges with
		| Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
		| AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
		| AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
		| Adherence           | 3      | 0      | 0    | 2014-08-11         |
	And I am american

Scenario: View available business hierarchy when data available is set to Site
	Given I have a role with
		| Field                 | Value |
		| Access to my site     | true  |
		| Access to Leaderboard | true  |
	And I am viewing leaderboard report
	When I open the hierarchy-picker
	Then I should see available business hierarchy
		| Value      |
		| Everyone   |
		| The site   |
		| Team green |
		| Team red   |

Scenario: Should only see myself on the leader board when has data available only for my own
	Given I have a role with
		| Field                 | Value |
		| Access to my own      | true  |
		| Access to Leaderboard | true  |
	When I am viewing leaderboard report
	Then I should see only myself on the leaderboard
	And I should not see hierarchy-picker

Scenario: Default leader board should be based on relative Everyone
	Given I have a role with
		| Field                 | Value |
		| Access to my team     | true  |
		| Access to Leaderboard | true  |
	When I am viewing leaderboard report
	Then The hierarchy-picker should have 'Everyone' selected 

Scenario: Should view correct leader board when selecting another group from different business hierarchy level
	Given I have a role with
		| Field                 | Value |
		| Access to Leaderboard | true  |
		| Access to my site     | true  |
	When I am viewing leaderboard report
	Then I should see the ranks are
		| Field        | value |
		| I            | 1     |
		| Pierre Baldi | 2     |
	When I select 'The site/Team red' in the hierarchy-picker
	Then I should see the ranks are
		| Field        | value |
		| Pierre Baldi | 1     |

