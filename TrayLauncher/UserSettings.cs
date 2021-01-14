// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;

namespace TrayLauncher
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            BackColor = 137;
            FirstRun = true;
            FontSize = 14;
            ForeColor = 7;
            HideOnStart = false;
            Icon = "red";
            MinimizeToTrayOnExit = false;
            SectionHeaderBold = true;
            SectionHeaderColor = 7;
            SeparatorColor = 24;
            ShadeAltRows = true;
            ShowItemType = false;
            StartNotification = true;
            StartWithWindows = false;
            VerifyExit = true;
            WindowHeight = 525;
            WindowLeft = 200;
            WindowTop = 200;
            WindowWidth = 800;
            XMLFile = string.Empty;
        }
        #endregion Constructor

        #region Properties
        public int BackColor
        {
            get { return backColor; }
            set
            {
                backColor = value;
                OnPropertyChanged();
            }
        }

        public bool FirstRun
        {
            get { return firstRun; }
            set
            {
                firstRun = value;
                OnPropertyChanged();
            }
        }

        public double FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                OnPropertyChanged();
            }
        }

        public int ForeColor
        {
            get { return foreColor; }
            set
            {
                foreColor = value;
                OnPropertyChanged();
            }
        }

        public bool HideOnStart
        {
            get { return hideOnStart; }
            set
            {
                hideOnStart = value;
                OnPropertyChanged();
            }
        }

        public string Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        public bool MinimizeToTrayOnExit
        {
            get { return minimizeToTrayOnExit; }
            set
            {
                minimizeToTrayOnExit = value;
                OnPropertyChanged();
            }
        }

        public bool SectionHeaderBold
        {
            get { return sectionHeaderBold; }
            set
            {
                sectionHeaderBold = value;
                OnPropertyChanged();
            }
        }

        public int SectionHeaderColor
        {
            get { return sectionHeaderColor; }
            set
            {
                sectionHeaderColor = value;
                OnPropertyChanged();
            }
        }

        public int SeparatorColor
        {
            get { return separatorColor; }
            set
            {
                separatorColor = value;
                OnPropertyChanged();
            }
        }

        public bool ShadeAltRows
        {
            get => shadeAltRows;
            set
            {
                shadeAltRows = value;
                OnPropertyChanged();
            }
        }

        public bool ShowItemType
        {
            get { return showItemType; }
            set
            {
                showItemType = value;
                OnPropertyChanged();
            }
        }

        public bool StartNotification
        {
            get { return startNotification; }
            set
            {
                startNotification = value;
                OnPropertyChanged();
            }
        }

        public bool StartWithWindows
        {
            get { return startWithWindows; }
            set
            {
                startWithWindows = value;
                OnPropertyChanged();
            }
        }

        public bool VerifyExit
        {
            get { return verifyExit; }
            set
            {
                verifyExit = value;
                OnPropertyChanged();
            }
        }

        public double WindowHeight
        {
            get
            {
                if (windowHeight < 100)
                {
                    windowHeight = 100;
                }
                return windowHeight;
            }
            set => windowHeight = value;
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 0;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 0;
                }
                return windowTop;
            }
            set => windowTop = value;
        }

        public double WindowWidth
        {
            get
            {
                if (windowWidth < 100)
                {
                    windowWidth = 100;
                }
                return windowWidth;
            }
            set => windowWidth = value;
        }

        private string xmlFile;

        public string XMLFile
        {
            get { return xmlFile; }
            set
            {
                xmlFile = value;
                OnPropertyChanged();
            }
        }
        #endregion Properties

        #region Private backing fields
        private int backColor;
        private bool firstRun;
        private double fontSize;
        private int foreColor;
        private bool hideOnStart;
        private string icon;
        private bool minimizeToTrayOnExit;
        private bool sectionHeaderBold;
        private int sectionHeaderColor;
        private int separatorColor;
        private bool shadeAltRows;
        private bool showItemType;
        private bool startNotification;
        private bool startWithWindows;
        private bool verifyExit;
        private double windowHeight;
        private double windowLeft;
        private double windowTop;
        private double windowWidth;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
