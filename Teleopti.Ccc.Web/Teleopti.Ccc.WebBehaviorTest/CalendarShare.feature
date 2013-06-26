Feature: Calendar Share
	In order to share my calendar with myself and others on the internet
	As an agent
	I want to 
		be able to activate calendar sharing on my settings
		be able to share my calendar with a calendar link

Background:
	Given there is a shift category named 'Day'
	And there is an activity with
	| Field | Value |
	| Name  | Phone |

Scenario: Activate calendar sharing
	Given I am an agent
	When I view my settings
	And I click 'share my calendar'
	Then I should see a sharing link

Scenario: View calendar sharing link
	Given 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-06-20 |
	And 'Pierre Baldi' have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-06-20 08:00 |
	| End time         | 2013-06-20 17:00 |
	And 'Pierre Baldi' has shared calendar
	When I am viewing sharing link
	Then I should see ical calendar with
	| Field    | Value            |
	| Activity | Phone            |
	| DTSTART  | 20130620T080000Z |
	| DTEND    | 20130620T170000Z |


