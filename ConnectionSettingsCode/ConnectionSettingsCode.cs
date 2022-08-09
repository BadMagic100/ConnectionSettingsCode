using Modding;
using System;

namespace ConnectionSettingsCode
{
    public class ConnectionSettingsCode : Mod
    {
        private static ConnectionSettingsCode? _instance;

        internal static ConnectionSettingsCode Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(ConnectionSettingsCode)} was never constructed");
                }
                return _instance;
            }
        }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public ConnectionSettingsCode() : base()
        {
            _instance = this;
        }
    }
}
