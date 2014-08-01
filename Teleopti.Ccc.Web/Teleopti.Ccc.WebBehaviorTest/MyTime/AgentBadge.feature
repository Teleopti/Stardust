@OnlyRunIfEnabled('MyTimeWeb_AgentBadge_28913')
Feature: AgentBadge
	As an agent I want to get motivated 
	by getting badges for my performance

Background: 
Given there is a role with
	| Field                    | Value                 |
	| Name                     | Full access to mytime |
  And I have the role 'Full access to mytime'
  And There is a agent badge settings with
  | Field        | Value |
  | BadgeEnabled | True  |

Scenario: Show my badge
Given I have badges with
| Badge type     | Bronze | Silver | Gold |
| Answered calls | 4      | 1      | 2    |
| AHT            | 2      | 1      | 1    |
| Adherence      | 4      | 0      | 0    |
 When I am viewing week schedule
 Then I should see I have 10 bronze badge, 2 silver badge and 3 gold badge
 When I view badge details
 Then I should see I have 4 bronze badges, 1 silver badge and 2 gold badge for Answered Calls
  And I should see I have 2 bronze badges, 1 silver badge and 1 gold badge for Average Handling Time
  And I should see I have 4 bronze badges, 0 silver badge and 0 gold badge for Adherence

Scenario: Show zero badge when agent has no badge
Given I have badges with
| Badge type     | Bronze | Silver | Gold |
 When I am viewing week schedule
 Then I should see I have 0 bronze badge, 0 silver badge and 0 gold badge
