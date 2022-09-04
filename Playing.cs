using System;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();
        private int defaultPort = 8080;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Playing";
            about.Description = "Serve a website with the playing song for Browser Sources";
            about.Author = "Assistant";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 0;
            about.Revision = 0;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = ReceiveNotificationFlags.PlayerEvents;
            about.ConfigurationPanelHeight = 0; // 25
            StartServer();
            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            //string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            //if (panelHandle != IntPtr.Zero)
            //{
            //    Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
            //    Label prompt = new Label();
            //    prompt.AutoSize = true;
            //    prompt.Location = new Point(0, 0);
            //    prompt.Text = "Port:";
            //    TextBox textBox = new TextBox();
            //    textBox.Bounds = new Rectangle(60, 0, 100, textBox.Height);
            //    configPanel.Controls.AddRange(new Control[] { prompt, textBox });
            //}
            return false;
        }
       
        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
            StopServer();
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
            StopServer();
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            switch (type)
            {
                case NotificationType.PluginStartup:
                case NotificationType.PlayStateChanged:
                case NotificationType.TrackChanged:
                    UpdateSong();
                    break;
            }
        }

        void UpdateSong()
        {
            if (mbApiInterface.Player_GetPlayState() == PlayState.Playing)
            {
                UpdateArtist(mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist));
                UpdateTitle(mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle));
            }
            else
            {
                UpdateArtist("");
                UpdateTitle("");
            }
        }

        void UpdateArtist(string value)
        {
            HttpServer.artist = value;
        }

        void UpdateTitle(string value)
        {
            HttpServer.title = value;
        }

        void StopServer()
        {
            HttpServer.Stop();
        }

        void StartServer()
        {
            HttpServer.port = defaultPort;
            HttpServer.Start();
        }
    }
}