@OnlyRunIfEnabled('SeatPlanner_Logon_32003')
@OnlyRunIfEnabled('Wfm_SeatPlan_SeatMapBookingView_32814')
@OnlyRunIfEnabled('WfmPeople_AdvancedSearch_32973')

Feature: SeatMapBookingView
	I want to view the seat bookings

Background: 
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Seat Planner     |
		| Access to seatplanner | True             |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And there is a seat in root location with
		| Field    | Value |
		| Name     | 1     |
		| Priority | 1     |
	And there is a seat booking for me
		| Field         | Value            |
		| BelongsToDate | 2015-01-01       |
		| StartDateTime | 2015-01-01 8:00  |
		| EndDateTime   | 2015-01-01 16:00 |
		| SeatName      | 1                |

	Given there is a site named 'London'
	And there is a team named 'Team Red' on site 'London'
	And there is a team named 'Team Blue' on site 'London'
	And I have a role with
	 | Field              | Value       |
	 | Name               | Team leader |
	 | Access to everyone | true        |
	 | Access to people   | true        |
	And Ashley Andeen has a person period with
	 | Field      | Value      |
	 | Team       | Team Red   |
	 | Start Date | 2015-01-01 |
	And Ashley Smith has a person period with
	 | Field      | Value      |
	 | Team       | Team Blue  |
	 | Start Date | 2015-01-01 |
	And I have a person period with
	 | Field      | Value      |
	 | Team       | Team Red   |
	 | Start Date | 2015-01-01 |


Scenario: be able to open the seat map booking view
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the seat map booking should be opened

Scenario: be able to go back to seat plan
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I press back button on seat booking view
	Then I should go back to seat plan

Scenario: the date is the same with I chosed
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the date of datepicker should be correct

Scenario: be able to view booking detail of selected seat
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I am able to view booking detail of selected seat

Scenario: be able to delete seat booking for the first seat
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I delete the first record under the seat booking details

Scenario: be able to open people search list
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	Then I should see people search list

Scenario: be able to search people with keyword
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	And I search people with keyword 'sm'
	Then I should see 'Smith' in people list

Scenario: the action button of people is invisible
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	Then I should not see the action buttons of people

Scenario: be able to close people list
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I click add agents to seat button
	And I click cancel button after open people search list
	Then I should not see people search list