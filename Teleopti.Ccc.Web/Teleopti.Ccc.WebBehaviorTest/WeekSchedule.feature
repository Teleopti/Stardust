Feature: View week schedule
	In order to know how to work this week
	As an agent
	I want to see my schedule details
	
Scenario: View current week
	Given I am an agent
	And My schedule is published
	When I view my week schedule
	Then I should see the start and end dates for current week

Scenario: View night shift
	Given I am an agent
	And I have a night shift starting on monday
	And My schedule is published
	When I view my week schedule
	Then the shift should end on monday

Scenario: Do not show unpublished schedule
	Given I am an agent
	And I have shifts scheduled for two weeks
	And My schedule is not published
	When I view my week schedule
	Then I should not see any shifts
	
Scenario: Do not show unpublished schedule for part of week
	Given I am an agent
	And I have shifts scheduled for two weeks
	And My schedule is published until wednesday
	When I view my week schedule
	Then I should not see any shifts after wednesday
	
Scenario: View meeting
	Given I am an agent
	# why is a shift required to show a meeting? A bug?
	And I have a shift on thursday
	And I have a meeting scheduled on thursday
	And My schedule is published
	When I view my week schedule
	And I click on the meeting
	Then I should see the meeting details

Scenario: View public note
	Given I am an agent
	And I have a public note on tuesday
	And My schedule is published
	When I view my week schedule
	Then I should see the public note on tuesday
	
Scenario: Select week from week-picker
	Given I am an agent
	And My schedule is published
	And I view my week schedule
	When I open the week-picker
	And I click on any day of a week
	Then the week-picker should close
	And I should see the selected week

Scenario: Week-picker monday first day of week for swedish culture
	Given I am an agent
	And I am swedish
	And I view my week schedule
	When I open the week-picker
	Then I should see monday as the first day of week

Scenario: Week-picker sunday first day of week for US culture
	Given I am an agent
	And I am american
	And I view my week schedule
	When I open the week-picker
	Then I should see sunday as the first day of week


	
Scenario: Show text request symbol
	Given I am an agent
	And I have an existing text request
	When I view my week schedule
	Then I should see a symbol at the top of the schedule
	And I should see a number with the request count

Scenario: Multiple day text requests symbol
	Given I am an agent
	And I have an existing text request spanning over 2 days
	When I view my week schedule
	Then I should see a symbol at the top of the schedule for the first day

Scenario: Show both text and absence requests
	Given I am an agent
	And I have an existing text request
	And I have an existing absence request
	When I view my week schedule
	Then I should see 2 with the request count

Scenario: Navigate to request page by clicking request symbol
	Given I am an agent
	And I have an existing text request
	When I view my week schedule
	And I click the request symbol
	Then I should see request page


Scenario: Navigate to current week
	Given I am an agent
	And I view my week schedule one month ago
	When I click the current week button
	Then I should see the start and end dates for current week



Scenario: Show timeline with no schedule
	Given I am an agent
	When I view my week schedule
	Then I should see start timeline and end timeline according to schedule with:
	| Field          | Value |
	| start timeline | 0:00  |
	| end timeline   | 23:59 |
	| timeline count | 25    |

Scenario: Show timeline with schedule
	Given I am an agent
	And I have shifts scheduled for two weeks
	And My schedule is published
	When I view my week schedule
	Then I should see start timeline and end timeline according to schedule with:
	| Field          | Value |
	| start timeline | 20:00 |
	| end timeline   | 4:00  |
	| timeline count | 9     |

Scenario: Show timeline with schedule with different start and end time on different day
	Given I am an agent
	And I have shifts scheduled with different activities for two weeks
	And My schedule is published
	When I view my week schedule
	Then I should see start timeline and end timeline according to schedule with:
	| Field          | Value |
	| start timeline | 8:00  |
	| end timeline   | 18:00 |
	| timeline count | 11     |

Scenario: Show activity with correct position, height and color
	Given I am an agent
	And I have custom shifts scheduled on wednesday for two weeks:
	| Field      | Value       |
	| Phone      | 09:00-10:30 |
	| Shortbreak | 10:30-11:00 |
	| Phone      | 11:00-12:00 |
	| Lunch      | 12:00-14:00 |
	| Phone      | 14:00-18:00 |
	And My schedule is published
	When I view my week schedule
	Then I should see wednesday's activities:
	| Activity   | Start Position | Height | Color |
	| Phone      | 67             | 99px   | Green |
	| Shortbreak | 167            | 32px   | Red   |