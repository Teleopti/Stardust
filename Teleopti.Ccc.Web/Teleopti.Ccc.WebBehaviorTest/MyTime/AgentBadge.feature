Feature: AgentBadge
	As an agent I want to get motivated
	by getting badges for my performance

Background: 
  Given there is a role with
		| Field | Value                 |
		| Name  | Full access to mytime |
	And I have the role 'Full access to mytime'
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And I have a person period with
		| Field      | Value      |
		| Team       | Team green |
		| Start Date | 2014-01-06 |
	And There is a gamification setting with
		| Field                 | Value   |
		| Description           | setting |
		| AnsweredCallsUsed     | True    |
		| AHTUsed               | True    |
		| AdherenceUsed         | True    |
		| Silver to bronze rate | 5       |
		| Gold to silver rate   | 5       |
		| RollingPeriodSet      | Monthly  |
	And There are teams applied with settings with
		| Team       | GamificationSetting |
		| Team green | setting             |

Scenario: Show my badge
Given I have today badges with
  | Badge type          | Bronze | Silver | Gold |
  | AnsweredCalls       | 4      | 1      | 2    |
  | AverageHandlingTime | 2      | 1      | 1    |
  | Adherence           | 3      | 0      | 0    |
 When I am viewing week schedule
 Then I should see I have 9 bronze badge, 2 silver badge and 3 gold badge
 When I view badge details
 Then I should see I have 4 bronze badges, 1 silver badge and 2 gold badge for AnsweredCalls
  And I should see I have 2 bronze badges, 1 silver badge and 1 gold badge for AverageHandlingTime
  And I should see I have 3 bronze badges, 0 silver badge and 0 gold badge for Adherence

  @NotKeyExample
Scenario: Show zero badge when agent has no badge
Given I have badges based on the specific setting with
  | Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
 When I am viewing week schedule
 Then I should see I have 0 bronze badge, 0 silver badge and 0 gold badge

Scenario: Show my badge within period
Given I have today badges with
  | Badge type          | Bronze | Silver | Gold |
  | AnsweredCalls       | 4      | 1      | 2    |
  | AverageHandlingTime | 2      | 1      | 1    |
  | Adherence           | 3      | 0      | 0    |
 When I am viewing week schedule
 Then I should see I have 9 bronze badge, 2 silver badge and 3 gold badge
 When I view badge details
  Then I should see I have 4 bronze badges, 1 silver badge and 2 gold badge for AnsweredCalls
  And I should see I have 2 bronze badges, 1 silver badge and 1 gold badge for AverageHandlingTime
  And I should see I have 3 bronze badges, 0 silver badge and 0 gold badge for Adherence

Scenario: Show my badge in next period
Given I have today badges with
  | Badge type          | Bronze | Silver | Gold |
  | AnsweredCalls       | 4      | 1      | 2    |
  | AverageHandlingTime | 2      | 1      | 1    |
  | Adherence           | 3      | 0      | 0    |
When I am viewing week schedule
Then I should see I have 9 bronze badge, 2 silver badge and 3 gold badge
When I view badge details
And I click next period
Then I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for AnsweredCalls
And I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for AverageHandlingTime
And I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for Adherence

Scenario: Show my badge in previous period
Given I have today badges with
  | Badge type          | Bronze | Silver | Gold |
  | AnsweredCalls       | 4      | 1      | 2    |
  | AverageHandlingTime | 2      | 1      | 1    |
  | Adherence           | 3      | 0      | 0    |
When I am viewing week schedule
Then I should see I have 9 bronze badge, 2 silver badge and 3 gold badge
When I view badge details
And I click previous period
Then I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for AnsweredCalls
And I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for AverageHandlingTime
And I should see I have 0 bronze badges, 0 silver badge and 0 gold badge for Adherence