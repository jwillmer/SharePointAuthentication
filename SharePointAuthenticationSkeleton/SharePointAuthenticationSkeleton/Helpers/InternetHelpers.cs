using System;
using Windows.Networking.Connectivity;

namespace SharePointAuthenticationSkeleton.Helpers
{
    /// <summary>
    /// From Olivier Matis (guruumeditation.net)
    /// http://www.gurumeditations.net/blog/internet-connection-type-detection-in-winrt
    /// </summary>
    public class InternetHelpers
    {
        public delegate void InternetConnectionChangedHandler(object sender, InternetConnectionChangedEventArgs args);

        public static event InternetConnectionChangedHandler InternetConnectionChanged;

        static InternetHelpers()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformationStatusChanged;
        }

        private static void NetworkInformationStatusChanged(object sender)
        {
            var arg = new InternetConnectionChangedEventArgs { IsConnected = (NetworkInformation.GetInternetConnectionProfile() != null), ConnectionType = InternetConnectionType };

            if (InternetConnectionChanged != null)
                InternetConnectionChanged(null, arg);
        }

        public static bool IsConnected { get { return NetworkInformation.GetInternetConnectionProfile() != null; } }


        public static InternetConnectionType InternetConnectionType
        {
            get
            {
                var currentconnection = NetworkInformation.GetInternetConnectionProfile();
                if (currentconnection == null)
                    return InternetConnectionType.None;
                switch (currentconnection.NetworkAdapter.IanaInterfaceType)
                {
                    case 6:
                        return InternetConnectionType.Cable;
                    case 71:
                        return InternetConnectionType.Wifi;
                    case 243:
                        return InternetConnectionType.Mobile;
                }

                return InternetConnectionType.Unknown;
            }
        }

    }




    public class InternetConnectionChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }

        public InternetConnectionType ConnectionType { get; set; }
    }

    public enum InternetConnectionType
    {
        None,
        Cable,
        Wifi,
        Mobile,
        Unknown
    }
}
