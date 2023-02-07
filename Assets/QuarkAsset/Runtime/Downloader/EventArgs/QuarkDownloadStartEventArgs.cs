using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadStartEventArgs : EventArgs, IRecyclable
    {
        public string Url { get; private set; }
        public string DownloadPath { get; private set; }
        public void Clear()
        {
            Url = string.Empty;
            DownloadPath = string.Empty;
        }
        internal static QuarkDownloadStartEventArgs Create(string url,string downloadPath)
        {
            var eventArgs= QuarkPool.Acquire<QuarkDownloadStartEventArgs>();
            eventArgs.Url= url;
            eventArgs.DownloadPath = downloadPath;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadStartEventArgs eventArgs)
        {
            QuarkPool.Release(eventArgs);
        }
    }
}
