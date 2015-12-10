@ignore
Feature: SeeWholeTeamSchedule
	As a team leader works in a big team
	I want to be able to see schedules of my whole team

Scenario: Can change page size
	Given I am a team leader with 25 members in my team
	And I am view my team schedules
	And I can see 18 agents in first page by default
	When I change page size to 30
	Then I can see all my team members