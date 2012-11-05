@ignore
Feature: Browser Notifications
	In order to get attention of system changes
	As an agent
	I want to get notified

Scenario: Window bar notification
	Given I have the role 'Full access to mytime'
	And Current time is '2000-01-01 12:00'
	And I have a shift from 9:00 to 17:00
	When I am viewing requests
	And My schedule between '2000-01-01 08:00' to '2000-01-01 17:00' change
	Then I should get a notification in the window

Scenario: Minimized IE8 notification
	Given I have the role 'Full access to mytime'
	And Current time is '2000-01-01 12:00'
	And I have a shift from 9:00 to 17:00
	When I am viewing requests
	And I have minimized the application
	And My schedule between '2000-01-01 08:00' to '2000-01-01 17:00' change
	Then The application icon in task bar should flash

Scenario: Minimized IE9/10 notification
	Given I have the role 'Full access to mytime'
	And Current time is '2000-01-01 12:00'
	And I have a shift from 9:00 to 17:00
	And I have pinned the application
	When I am viewing requests
	And I have minimized the application
	And My schedule between '2000-01-01 08:00' to '2000-01-01 17:00' change
	Then The application icon in task bar should flash

Scenario: Minimized Chrome notification
	Given I have the role 'Full access to mytime'
	And Current time is '2000-01-01 12:00'
	And I have a shift from 9:00 to 17:00
	And I have accepted web notification at logon
	When I am viewing requests
	And I have minimized the application
	And My schedule between '2000-01-01 08:00' to '2000-01-01 17:00' change
	Then I should get a desktop notification

Scenario: Minimized Firefox notification
	Given I have the role 'Full access to mytime'
	And Current time is '2000-01-01 12:00'
	And I have a shift from 9:00 to 17:00
	And I have installed https://addons.mozilla.org/sv-SE/firefox/addon/tab-notifier/
	And I have accepted web notification at logon
	When I am viewing requests
	And I have minimized the application
	And My schedule between '2000-01-01 08:00' to '2000-01-01 17:00' change
	Then I should get a desktop notification
