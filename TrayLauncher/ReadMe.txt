The TrayLauncher ReadMe file


Introduction
============
TrayLauncher is customizable menu to launch applications, websites, documents, settings, folders
and progressive web apps (PWAs) from the system tray area of the taskbar. You can use the
TrayLauncher menu to launch your favorite applications, open frequently used folders and websites,
or to open that infrequently used program or setting that takes you minutes to locate.


How TrayLauncher Works
======================
When TrayLauncher is first run is will put an icon in the system tray and open its main window. It
will also show a message in the notification area. You can choose to disable the message and hide
the main window in the Options menu if desired.

Left-clicking the icon in the system tray will display the menu. This menu contains a few items to
get you started. As you would expect, clicking on the menu item will launch the application
associated with that item.

TrayLauncher was built to run in x64 mode to be able to launch certain Windows applications.


Main Window
===========
The main window is where you configure the tray menu. From there you can add, update, remove
and test menu items. You can also change the menu text color, background color and the color
of the menu separators. Additionally you can change the font size and the color of the tray
icon.

The grid below the menu contains the items that have been added to the TrayLauncher menu. Two
additional items will always appear on the menu. "Manage" appears at the top of the menu. Clicking
Manage will display the main window if it is hidden. "Exit" appears as the last item on the menu.
Clicking Exit will end the TrayLauncher application.

When minimized the main window will not show in the task bar. To reopen it, double-click the
tray icon or left-click and select Manage at the top of the menu.


Adding Entries
==============
To add a menu entry, select the Configuration menu in the main window and then select Add Menu Item.
A window will open.

Enter the name of the menu item in the Menu Item Text field. This is the text that will be displayed
on the tray menu. Every entry must have Menu Item Text.

Next, in the Application Path field, enter the path of what you want to open. This can be the path
to an application or batch file, the URL for a website, the path to a folder, or a windows setting.
Every entry except the menu separator must have an application path.

The next field is Arguments. This is where parameters are passed to the application. For
example if you wanted to have a menu entry for Device Manager, you would enter MMC.exe for the
application path and devmgmt.msc for the argument. This field is optional.

The ToolTip field is on optional field where you can specify text for a tooltip. A tooltip is
that little box of text that briefly appears when you hover the mouse cursor over a UI element.
This field is usually used to supply a clue as to what the menu entry does.

The final field is Position. The position value is used to determine where in the menu an entry
will be displayed. Generally speaking, lower values appear at the top of the menu and higher
values appear at or near the bottom. Any integer from 0 through 99999 can be used. The Position
field will only accept digits for input. If the same number is specified for multiple menu items,
they will be displayed consecutively in the menu. The Refresh List item on the Configuration menu
will renumber the list, starting at 100 and incrementing each item by 10. The Position field is
required for every menu item.

You can separate menu items into groups by placing a Separator between items. To add a separator
enter "Separator" in the Menu Item Text field and supply a value in the position field. Alternately
use the predefined separator from the drop-down discussed in the next paragraph. The other fields
are ignored for a separator. Note that tooltips are not visible for separators. Separators appear
grayed out in the main window as a visual cue.

Below the Position field is a drop-down list of predefined menu items. These items include folders,
shortcuts to Control Panel items, system settings and some Windows accessories. To add an item from
this list to the TrayLauncher menu, select the item from the drop-down and supply a position for
that item. Add some tooltip text if desired. The entry for the menu separator is at the top of this
list. The remaining items are sorted alphabetically.

When you are ready, click the Add button. Multiple menu entries can be added in this manner. When
you are done adding items, click the Done button.

After the item has been added you can test the item from the main window by selecting Test Selected
item from the Configuration menu.


Changing Entries
================
To change a menu item, highlight its entry in the main menu and then select Update Selected Item
from the Configuration menu.


Removing Entries
================
To remove a menu item, highlight its entry in the main menu and then select Remove Selected Item
from the Configuration menu.


The Context Menu
================
In addition to the Configuration menu, items may be added, updated, removed and tested from the
context menu that appears when you right-click on a menu item.


The Options Menu
================
From the Options menu you can choose to have TrayLauncher start with windows so that it will always
be available. You can choose to have a notification display when it starts. You can also choose to
have the main window be hidden when TrayLauncher starts (recommended).

The next section of the Options menu allows you to increase or decrease the font size of both the
TrayLauncher menu and the grid in the main window.

The color of the menu text, the menu background and of any separators can be changed in the next
section. There is also a menu item that shows a preview of the color choices. Choose a combination
that suits your personal taste.

Finally, there is an option to change the color of the icon in the tray. There are eight colors to
choose from. This will change the icon in the tray, but due to a quirk/feature in Windows the icon
displayed on the taskbar will stay the same as the icon used in the start menu.


Tips
====

If the TrayLauncher icon doesn't stay visible in the system tray, open the Settings app and
navigate to Personalization then to Taskbar then click on "Select which icons appear on the taskbar"
then toggle the switch next to TrayLauncher on.

To open a folder you can either supply the fully qualified path as the Application Path or use
Explorer.exe as the path and the fully qualified path as an Argument.

To open a document enter the fully qualified path in the Application Path (if the document type
has a default application).

If an application is in the Windows path there is no need to specify the fully qualified path.
For example there's no benefit in typing C:\Windows\System32\control.exe when conrtol.exe works
fine.

Environmental variables are resolved when used in the Application Path or Arguments fields. For
example, you could open the Windows folder by specifying %WINDIR% as the Application Path. You can
even build upon the variables. For example %WINDIR%\Fonts would open the Fonts folder.

After taking several minutes/hours/days to customize the menu with just the right items in just the
right order, take a moment to backup your hard work by selecting Backup Menu File from the File menu.

TrayLauncher should be able to launch anything that you can launch using the Run dialog. Heck,
there's even a predefined menu item for the Run dialog!

If you really dislike all of the tray icons, you can use your own. Navigate to TrayLauncher's folder
in Program Files (x86) and then to the Images folder. Rename one of the icons. Edit one of the
existing icons or copy a new icon into that folder and name it the original name of the one that
was renamed. Then select that icon from the Icon Color menu. Note that the new icon must be an ICO
file and not another image format renamed to ICO.

If the separators are showing on the menu as words instead of horizontal lines, check the spelling,
that first "a" can be tricky.


Diagnostics
===========
Press F2 to view the current settings. Pressing F3 will open the menu file in the default application
for XML files. F4 will open the temp file which contains diagnostic messages. If TrayLauncher
encounters an error, it will display an error message and also write messages to the temp file.


Notices and License
===================

TrayLauncher was written in C# by Tim Kennedy. Graphics & sound files were created by Tim Kennedy.

TrayLauncher uses Hardcodet.NotifyIcon.Wpf from Philipp Sumi.  http://www.hardcodet.net/wpf-notifyicon


MIT License
Copyright (c) 2019 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.