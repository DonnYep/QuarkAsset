using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadStartEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadNode CurrentDownloadNode { get; private set; }
        public override void Clear()
        {
            CurrentDownloadNode = default;
        }
        //internal QuarkDownloadStartEventArgs() { }
        internal static QuarkDownloadStartEventArgs Create(QuarkDownloadNode node)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkDownloadStartEventArgs>();
            eventArgs.CurrentDownloadNode = node;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadStartEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
