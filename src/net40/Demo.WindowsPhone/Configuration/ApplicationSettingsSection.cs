﻿using Microsoft.Practices.Mobile.Configuration;

namespace Demo.WindowsPhone.Configuration
{
    public class ApplicationSettingsSection : ConfigurationSection
    {
        /// <summary>
        /// Get the connection Items.
        /// </summary>
        [ConfigurationProperty("appSettings")]
        public AppSettingsElementCollection  AppSettings
        {
            get
            {
                return (AppSettingsElementCollection)(this["appSettings"]);
            }
        }
    }
}
