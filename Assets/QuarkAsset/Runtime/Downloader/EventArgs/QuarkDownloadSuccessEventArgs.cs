using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadSuccessEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadNode CurrentDownloadNode { get; private set; }
        public override void Clear()
        {
            CurrentDownloadNode = default;
        }
        //internal QuarkDownloadSuccessEventArgs() { }
        internal static QuarkDownloadSuccessEventArgs Create(QuarkDownloadNode node)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkDownloadSuccessEventArgs>();
            eventArgs.CurrentDownloadNode = node;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadSuccessEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
