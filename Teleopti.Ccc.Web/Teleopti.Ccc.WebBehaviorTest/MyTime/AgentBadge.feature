Feature: AgentBadge
	As an agent I want to get motivated 
	by getting badges for my performance

Background: 
Given I am an agent
  And there is a role with
   | Field | Value                 |
   | Name  | Full access to mytime |
  And I have the role 'Full access to mytime'

@ignore
Scenario: Show my badge
Given I have badges 
| KpiName               | BadgeType | Count |
| Answered calls        | Normal    | 5     |
| Answered calls        | Silver    | 2     |
| Answered calls        | Golden    | 1     |
| Average handling time | Normal    | 10    |
| Average handling time | Silver    | 1     |
| Average handling time | Golden    | 0     |
| Adherence             | Normal    | 4     |
| Adherence             | Silver    | 0     |
| Adherence             | Golden    | 0     |
 When I am viewing week schedule
 Then I should see I have badge

@ignore
Scenario: Show message when agent has no badge
Given I have badges
| KpiName               | BadgeType | Count |
| Answered calls        | Normal    | 0     |
| Answered calls        | Silver    | 0     |
| Answered calls        | Golden    | 0     |
| Average handling time | Normal    | 0     |
| Average handling time | Silver    | 0     |
| Average handling time | Golden    | 0     |
| Adherence             | Normal    | 0     |
| Adherence             | Silver    | 0     |
| Adherence             | Golden    | 0     |
 When I am viewing week schedule
 Then I should see I have no badge

@ignore
Scenario: Notify when agent get new badge
Given Badge threshold value for "Answered calls" set to 50
  And the current time is '2015-01-01 23:59:59'
  And My "Answered calls" for '2015-01-01' is 51
 When current browser time has changed to '2015-01-02 00:00:00'
  And I am viewing week schedule
 Then I should be notified I got a new badge
