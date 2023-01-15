using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAA_Project.Platforms.Android
{
    internal class Constants
    {
        public static string LocalhostUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        public static string Scheme = "https"; // or http
        public static string Port = "5001";
        public static string RestUrl = $"{Scheme}://{LocalhostUrl}:{Port}/emotion_record_binary/{{0}}";
    }
}
