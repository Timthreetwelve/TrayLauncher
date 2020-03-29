# The TrayLauncher ReadMe file



#### Introduction

TrayLauncher is a customizable menu to launch applications, websites, documents, settings, folders and progressive web apps (PWAs) from the system tray area of the taskbar. You can use the TrayLauncher menu to launch your favorite applications, open frequently used folders and websites, or to open that infrequently used program or setting that takes you minutes to locate. You can use TrayLauncher instead of having a desktop full of documents and shortcuts.



#### How TrayLauncher Works

When TrayLauncher is first run is will put an icon in the system tray and open its main window. It will also show a message in the notification area. You can choose to disable the message and hide the main window in Preferences if desired.

Clicking (left or right) the icon in the system tray will display the menu. This menu contains a few items to get you started. As you would expect, clicking on the menu item will launch the application associated with that item. Note that there is a slight delay to display the menu when left clicking. This delay is built into Windows and can be adjusted in *Control Panel > Mouse Properties > Double-click speed*.



#### Main Window

The main window is where you configure the tray menu. From there you can add, update, remove and test menu items. You can also change the menu text color, background color and the color of the menu separators. Additionally, you can change the font size of the menu items and the color of the tray icon.

The grid in the main window contains the items that have been added to the TrayLauncher menu. The items in the grid appear in the same order that they do in the menu. Two additional items will always appear on the menu. "Manage" appears at the top of the menu. Clicking Manage will display the main window if it is hidden. "Exit" appears as the last item on the menu. Clicking Exit will end the TrayLauncher application.

When minimized, the main window will not show in the task bar. To reopen it, double-click the tray icon or single-click the icon and select Manage at the top of the menu.



#### Adding Entries

To add a menu entry, select Menu Configuration in the main window and then select Add Menu Item.  A window will open.

Enter the name of the menu item in the **Menu Item Text** field. This is the text that will be displayed on the tray menu. Use a name that distinctive but short. Every entry must have Menu Item Text.

Note that the underscore character **"_"** has a special meaning in Windows menus. To provide the expected result, TrayLauncher will convert a single underscore to two consecutive underscore characters in the Add Item window, however menu will display a single underscore. This behavior applies to the Copy and Update windows as well.

Next, in the **Application Path** field, enter the path of what you want to open. This can be the path to an application or document, the URL for a website, the path to a folder, or a Windows setting. Every entry except menu separators and section headers must have an application path. Click the [...] button to the right of the field to use the open file dialog for applications or documents.

The next field is **Arguments**. This is where parameters are passed to the application. For example if you wanted to have a menu entry for Device Manager, you would enter MMC.exe for the application path and devmgmt.msc for the argument. This field is only required if the application requires it.

The **ToolTip** field is on optional field where you can specify text for a tooltip. A tooltip is that little box of text that briefly appears when you hover the mouse cursor over a UI element. This field is usually used to supply a clue as to what the menu entry does.

Under the Tooltip field is a row of radio buttons that indicate the item type. These buttons will automatically update when adding items using the Special Items drop-down.

The final field is **Position**. The position value is used to determine where in the menu an entry will be displayed. Lower values appear at the top of the menu and higher values appear at or near the bottom. Any integer from 0 through 99999 can be used. The Position field will only accept digits for input. If the same number is specified for multiple menu items, they will be displayed consecutively in the menu. The Refresh List item on the Configuration menu will renumber the list, starting at 100 and incrementing each item by 10. The Position field is required for every menu item.

Next to the Position field is a drop-down list of **predefined menu items**. These items include folders, shortcuts to Control Panel items, system settings and some Windows accessories. To use one of these entries simply click the drop-down labeled Select a menu item. Scroll to find the entry that you want and click that entry. Except for the Position field, the required fields will be filled in. Just add a position and click the Add button.

The first entry in the Special Items drop-down is **Separator**. A separator is a thin horizontal line between menu items. To add a separator select it from the drop-down and give it a position number, all the other fields are irrelevant.

The next item is **Section Name**. Menu items can be grouped into sections using separators and section headers. To add a section header select it and give it a position. Then change the text in the Menu Item Text field to reflect the name of the section you are creating.

The next item is **Sub Menu.** A  sub menu is a menu that branches off from the primary menu. To add a sub menu select it and give it a position. Then change the text in the Menu Item Text field to reflect the name of the sub menu you are creating.

The next item is **Sub Menu Item.** Sub menu items are like normal items except that they will be shown in the sub menu. To add a sub menu item select it and give it a position. The position must be directly under a sub menu or another sub menu item.

The remaining items are the predefined menu items discussed above.

When you are ready, click the **Add button**. Multiple menu entries can be added in this manner. When you are done adding items, click the **Done** button.

After the item has been added you can test the item from the main window by selecting Test Selected item from the Configuration menu.

#### Copying Entries

To copy a menu item, highlight its entry in the main window and then select Copy Menu Item from the 
Configuration menu. After making the desired changes, click the Copy button in the Copy window.

#### Changing Entries

To change or update a menu item, highlight its entry in the main window and then select **Update Selected Item** from the Configuration menu. An item may also be updated by double-clicking that item. After making the desired changes, click the Update button in the Update window.



#### Removing Entries

To remove a menu item, highlight its entry in the main menu and then select **Remove Selected Item** from the Configuration menu or press the Delete key.



#### The Context Menu

In addition to the Configuration menu, items may be added, updated, removed and tested from the context menu that appears when you right-click on a menu item in the main window. Testing a menu item is the same as clicking on it in the menu, just a little more convenient.



#### Preferences

Open the **Preferences** dialog by clicking the last selection on the Menu Configuration menu or by pressing Ctrl plus comma.

From Preferences you can choose to have TrayLauncher start with windows so that it will always be available (*recommended*).

You can choose to have a notification display when TrayLauncher starts.

You can also choose to have the main window be hidden when TrayLauncher starts (*recommended*).

When selected, the "Minimize to Tray on Exit" option will hide the main window instead of exiting the application when the "X" on the title bar is clicked.

The Confirm Main Window Exit option controls whether or not you will be asked to confirm your intention to exit the application when clicking the X on the title bar.

Note that you can select either the Minimize or the Confirm options, *but not both*.

The next item on the Options menu will show or hide the Type column in the main window. They type column can be helpful when creating sub menus and sub menu items.

The Shade Alternate Rows option will apply subtle shading to every other row of the grid.

The Font Size slider allows you to increase or decrease the font size of both the TrayLauncher menu and the grid in the main window.

Starting at the top of the right section there is an option to change the color of the icon in the tray. There are twelve colors to choose from. This will change the color of the icon in the tray, but due to a quirk/feature in Windows the icon displayed on the taskbar will stay the same as the icon used in the start menu.

The color of the menu text, the menu background, the section headers, and separators can be individually changed using the dropdowns. Choose a combination that suits your personal taste.

Under the color options is an option for bold section headers.

Finally, there are six predefined color themes.

Note that settings are saved when the Preferences window is closed.



#### Help

In addition to the About dialog, the Help menu has options to view this readme file as well as the Change log.

From the Debug sub menu you can view the current settings. You can also open the menu file in the default application for XML files or open the temp file which contains diagnostic messages.

If TrayLauncher encounters an error, it will display an error message and write messages to the temp file.



#### Tips

If the TrayLauncher icon doesn't stay visible in the system tray, open the Settings app and navigate to Personalization then to Taskbar then click on "Select which icons appear on the taskbar" then toggle the switch next to TrayLauncher on.

To open a folder, you can either supply the fully qualified path as the Application Path or use Explorer.exe as the path and the fully qualified path as an Argument.

Make use of sub-menus to keep the main menu from becoming too cluttered.

To open a document, enter the fully qualified path in the Application Path. There is no need to specify the executable if you want the document to open in the default application.

If an application is in the Windows path there is no need to specify the fully qualified path. For example, there's no benefit in typing C:\Windows\System32\control.exe when conrtol.exe will do

Environmental variables are resolved when used in the Application Path or Arguments fields. For example, you could open the Windows folder by specifying %WINDIR% as the Application Path. You can even build upon the variables. For example, %WINDIR%\Fonts would open the Fonts folder.

After taking several minutes/hours/days to customize the menu with just the right items in just the right order, take a moment to back up your hard work by selecting Backup Menu File from the File menu. If your menu ever gets messed up, you restore it by replacing it with the backup file.

TrayLauncher should be able to launch anything that you can launch using the built-in Run dialog.

If you really dislike all the tray icons, you can use your own. Navigate to TrayLauncher's folder in Program Files and then to the Images folder. Rename one of the icons. Edit one of the existing icons or copy a new icon into that folder and name it the original name of the one that was renamed. Then select that icon color in Preferences. Note that the new icon must be an ICO file and not another image format.



#### Notices and License

TrayLauncher was written in C# by Tim Kennedy. Graphics & sound files were created by Tim Kennedy.

TrayLauncher uses Hardcodet.NotifyIcon.Wpf from Philipp Sumi.  http://www.hardcodet.net/wpf-notifyicon

**MIT License**
**Copyright (c) 2019 - 2020 Tim Kennedy**

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.