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
	And I have shared my calendar
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
	And I have shared my calendar
	When I view my settings
	Then I should see 'share my calendar' active
	When I click 'revoke'
	Then I should see 'share my calendar' inactive
	And I should not see a sharing link

Scenario: Dont share calendar when sharing is revoked
	Given I have the role 'Access to CalendarLink'
	And I have revoked calendar sharing
	When Someone is viewing sharing link
	Then Someone should not see ical calendar

Scenario: Dont share calendar when permission is revoked
	Given I have the role 'No access to CalendarLink'
	And I have shared my calendar before
	When Someone is viewing sharing link
	Then Someone should not see ical calendar

Scenario: View calendar sharing link
	Given I have the role 'Access to CalendarLink'
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2040-06-24         |
	And the current time is '2023-06-20'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2022-04-10 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with
	| Field      | Value      |
	| Start date | 2023-04-10 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-04-20 08:00 |
	| End time         | 2023-04-20 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-04-21 08:00 |
	| End time         | 2023-04-21 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-12-17 08:00 |
	| End time         | 2023-12-17 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-12-18 08:00 |
	| End time         | 2023-12-18 17:00 |
	And I have shared my calendar
	When Someone is viewing sharing link
	Then Someone should see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20230421T080000Z |
	| DTEND   | 20230421T170000Z |
	And Someone should see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20231217T080000Z |
	| DTEND   | 20231217T170000Z |
	And Someone should not see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20130420T080000Z |
	| DTEND   | 20130420T170000Z |
	And Someone should not see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20131218T080000Z |
	| DTEND   | 20131218T170000Z |

Scenario: Cannot view unpublished calendar
	Given I have the role 'Access to CalendarLink'
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2023-06-24         |
	And the current time is '2023-06-20'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2023-04-10 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with
	| Field      | Value      |
	| Start date | 2023-04-10 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-06-24 08:00 |
	| End time         | 2023-06-24 17:00 |
	And I have a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| Start time       | 2023-06-25 08:00 |
	| End time         | 2023-06-25 17:00 |
	And I have shared my calendar
	When Someone is viewing sharing link
	Then Someone should see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20230624T080000Z |
	| DTEND   | 20230624T170000Z |
	And Someone should not see ical calendar with
	| Field   | Value            |
	| SUMMARY | Phone            |
	| DTSTART | 20230625T080000Z |
	| DTEND   | 20230625T170000Z |