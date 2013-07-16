Feature: Calendar Share
	In order to share my calendar with myself and others on the internet
	As an agent
	I want to 
		be able to activate calendar sharing on my settings
		be able to share my calendar with a calendar link

Background:
	Given there is a shift category named 'Day'
	And there is a role with
	| Field                  | Value                  |
	| Name                   | Access to CalendarLink |
	| Access to CalendarLink | true                   |
	And there is a role with
	| Field                  | Value                     |
	| Name                   | No access to CalendarLink |
	| Access to CalendarLink | false                     |
	And there is an activity with
	| Field | Value |
	| Name  | Phone |

Scenario: Cannot share calendar without permission
	Given I have the role 'No access to CalendarLink'
	When I view my settings
	Then I should not see 'share my calendar' in settings

Scenario: View calendar sharing activation status
	Given I have the role 'Access to CalendarLink'
	And I have shared calendar
	When I view my settings
	Then I should see 'share my calendar' active
	And I should see a sharing link

Scenario: Activate calendar sharing
	Given I have the role 'Access to CalendarLink'
	When I view my settings
	And I click 'share my calendar'
	Then I should see a sharing link

Scenario: Revoke calendar sharing
	Given I have the role 'Access to CalendarLink'
	And I have shared calendar
	When I view my settings
	Then I should see 'share my calendar' active
	When I click 'revoke'
	Then I should see 'share my calendar' inactive
	And I should not see a sharing link

Scenario: Stop calendar sharing after revoke
	Given I have the role 'Access to CalendarLink'
	And I have shared calendar
	When I view my settings
	Then I should see 'share my calendar' active
	When I click 'revoke'
	And Someone is viewing sharing link
	Then Someone should not see ical calendar

Scenario: View calendar sharing link
	Given I have the role 'Access to CalendarLink'
	And Current time is '2013-06-20'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-04-10 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with
	| Field      | Value      |
	| Start date | 2013-04-10 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-04-19 08:00 |
	| End time         | 2013-04-19 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-04-20 08:00 |
	| End time         | 2013-04-20 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-12-19 08:00 |
	| End time         | 2013-12-19 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2013-12-20 08:00 |
	| End time         | 2013-12-20 17:00 |
	And I have shared calendar
	When Someone is viewing sharing link
	Then Someone should see ical calendar with
	| Field    | Value            |
	| Activity | Phone            |
	| DTSTART  | 20130420T080000Z |
	| DTEND    | 20130420T170000Z |
	And Someone should see ical calendar with
	| Field    | Value            |
	| Activity | Phone            |
	| DTSTART  | 20131219T080000Z |
	| DTEND    | 20131219T170000Z |
	And Someone should not see ical calendar with
	| Field    | Value            |
	| Activity | Phone            |
	| DTSTART  | 20130419T080000Z |
	| DTEND    | 20130419T170000Z |
	And Someone should not see ical calendar with
	| Field    | Value            |
	| Activity | Phone            |
	| DTSTART  | 20131220T080000Z |
	| DTEND    | 20131220T170000Z |

