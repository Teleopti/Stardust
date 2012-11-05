@ignore
Feature: Browser Notifications
	In order to get attention of system changes
	As an agent
	I want to get notified

Scenario: Window bar notification
	Given I have the role 'Full access to mytime'
	When Today's schedule change
	Then I should get a notification in the window

Scenario: Minimized IE8 notification
	Given I have the role 'Full access to mytime'
	And I have minimized the application
	When Today's schedule change
	Then The application icon in task bar should flash

Scenario: Minimized IE9/10 notification
	Given I have the role 'Full access to mytime'
	And I have pinned the application
	And I have minimized the application
	When Today's schedule change
	Then The application icon in task bar should flash

Scenario: Minimized Chrome notification
	Given I have the role 'Full access to mytime'
	And I have accepted web notification at logon
	And I have minimized the application
	When Today's schedule change
	Then I should get a desktop notification

Scenario: Minimized Firefox notification
	Given I have the role 'Full access to mytime'
	And I have installed https://addons.mozilla.org/sv-SE/firefox/addon/tab-notifier/
	And I have accepted web notification at logon
	And I have minimized the application
	When Today's schedule change
	Then I should get a desktop notification
