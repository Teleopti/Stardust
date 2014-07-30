@OnlyRunIfEnabled('MyTimeWeb_AgentBadge_28913')
Feature: AgentBadge
	As an agent I want to get motivated 
	by getting badges for my performance

Background: 
Given I am an agent

@ignore
Scenario: Show my badge
Given I have badges with
| Badge type     | Bronze | Silver | Gold |
| Answered calls | 4      | 1      | 2    |
| AHT            | 2      | 1      | 1    |
| Adherence      | 4      | 0      | 0    |
 When I am viewing week schedule
 Then I should see I have 10 bronze badge, 2 silver badge and 3 gold badge

@ignore
Scenario: Show message when agent has no badge
Given I have badges with
| Badge type     | Bronze | Silver | Gold |
| Answered calls | 0      | 0      | 0    |
| AHT            | 0      | 0      | 0    |
| Adherence      | 0      | 0      | 0    |
 When I am viewing week schedule
 Then I should see I have 0 bronze badge, 0 silver badge and 0 gold badge

@ignore
Scenario: Notify when agent get new badge
Given Badge threshold value for "Answered calls" set to 50
  And the current time is '2015-01-01 23:59:59'
  And My "Answered calls" for '2015-01-01' is 51
 When current browser time has changed to '2015-01-02 00:00:00'
  And I am viewing week schedule
 Then I should be notified I got a new badge
