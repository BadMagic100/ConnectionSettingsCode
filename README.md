# ConnectionSettingsCode

A Hollow Knight mod that enables simple settings sharing via MenuChanger menus.

Provides the `SettingsCode` menu element for mod authors to use. By default, the element
will be placed in the bottom left, to the side of the back button, in your connection page.
Clicking the "Copy Settings" button generates a standardized settings code based on the provided elements
and your mod, which is automatically copied to the clipboard buffer. Clicking the "Apply Settings" button
attempts to read the settings code from the clipboard, and after performing some validation, applies the
settings from the code to the menu elements (and their underlying data) automatically.
