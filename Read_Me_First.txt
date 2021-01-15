Instructions for updating TrayLauncher
======================================

If TrayLauncher is not currently installed disregard these instructions.


Before Installing
=================

1. Do not uninstall current version!

2. Start the current version of TrayLauncher if it's not already running and then open the main window.

3. Backup the menu configuration file by opening the File menu and selecting Backup Menu File. Save the backup file somewhere convenient, you will need it later.

4. Exit TrayLauncher by selecting Exit from the File menu so that it doesn't minimize to the system tray.


Convert Settings File
=====================

5. Download the ConvertMySettings program from GitHub. https://github.com/Timthreetwelve/ConvertMySettings/releases/download/v0.1.1/ConvertMySettings_app.zip

6. Unzip ConvertMySettings to a folder of your choice.

7. Run ConvertMySettings and follow the instructions on the screen. It will save a JSON file on the desktop.


Install the New Version
=======================

8. Install the new version of TrayLauncher but DO NOT start it.

9. Copy the JSON file that was created in step 7 into the TrayLauncher folder located in %localappdata%\Programs\T_K\TrayLauncher (copy the path and paste into the Run dialog (Win+R)).

10. Rename the JSON file to UserSettings.json. Case doesn't matter.


Start TrayLauncher and Restore Menu File
========================================

11. Now you can start the new version of TrayLauncher.

12. Open the File menu and select Restore Menu File. Select the backup file created in step 3 and then click Open.

13. Open the Menu Configuration menu and select Preferences. Verify that the preferences are correct.

14. If you have want TrayLauncher to start with Windows, uncheck the Start with Windows option, then check it again.

15. Make sure everything is working correctly before proceeding to the next step.


Cleanup Previous Settings
=========================

16. Open %localappdata%\T_K in File Explorer (copy the path and paste into the Run dialog (Win+R)).

17. Delete the folder that begins with TrayLauncher. This folder is no longer needed. The settings files are now kept in %localappdata%\Programs\T_K\TrayLauncher. If the TrayLauncher folder was the only folder in the T_K folder then it may be deleted as well.

That's it!


Now that settings are being kept in %localappdata%\Programs\T_K\ you may wish to include this folder in your backup routine.

Thanks
