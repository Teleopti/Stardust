Feature: Status
  In order to see how system health is
  As a non logged on user
  I want to see current status

Scenario: Check that data binding works on status page
	When I view status page
	Then Status page should say ok