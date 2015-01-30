# MyTimeWeb_AgentBadge_28913 is mutual with Portal_DifferentiateBadgeSettingForAgents_31318
# So they should not be true at the same time.
# And Portal_DifferentiateBadgeSettingForAgents_31318 has higher priority, as long as it is enabled, code will always go with its flow.
# To make it not impact the build, ignore the whole feature with old toggle.
@OnlyRunIfEnabled('MyTimeWeb_AgentBadge_28913')
Feature: AgentBadge
	As an agent I want to get motivated
	by getting badges for my performance

Background: 
Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
  And I have the role 'Full access to mytime'

Scenario: Show my badge
Given There is an agent badge settings with
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
  | Adherence           | 3      | 0      | 0    | 2014-08-11         |
 When I am viewing week schedule
 Then I should see I have 9 bronze badge, 2 silver badge and 3 gold badge
 When I view badge details
 Then I should see I have 4 bronze badges, 1 silver badge and 2 gold badge for AnsweredCalls
  And I should see I have 2 bronze badges, 1 silver badge and 1 gold badge for AverageHandlingTime
  And I should see I have 3 bronze badges, 0 silver badge and 0 gold badge for Adherence

Scenario: Show zero badge when agent has no badge
Given There is an agent badge settings with
  | Field                 | Value |
  | BadgeEnabled          | True  |
  | AnsweredCallsUsed     | True  |
  | Silver to bronze rate | 5     |
  | Gold to silver rate   | 2     |
  And I have badges with
  | Badge type     | Bronze | Silver | Gold |
 When I am viewing week schedule
 Then I should see I have 0 bronze badge, 0 silver badge and 0 gold badge

Scenario: Do not show badge when badge feature disabled
Given There is an agent badge settings with
  | Field                 | Value |
  | BadgeEnabled          | False |
  | Silver to bronze rate | 5     |
  | Gold to silver rate   | 2     |
  And I have badges with
  | Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
  | AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
  | AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
  | Adherence           | 3      | 0      | 0    | 2014-08-11         |
 When I am viewing week schedule
 Then There should display no badge information
