Feature: Seat Map
	As a seat planner
	I want to manage locations and seats

Scenario: create a seat
	Given I have a role with
		| Field                 | Value        |
		| Name                  | Seat Planner |
		| Access to seatplanner | True              |
	When I view Seat map
	And I create a seat
	Then I should see a seat in the seat map

Scenario: create a location
	Given I have a role with
		| Field                 | Value        |
		| Name                  | Seat Planner |
		| Access to seatplanner | True              |
	When I view Seat map
	And I create a location
	Then I should see a location in the seat map
	