﻿using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadingEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadNode CurrentDownloadNode { get; private set; }
        /// <summary>
        /// 当前资源下载的序号；
        /// </summary>
        public int CurrentDownloadIndex { get; private set; }
        /// <summary>
        /// 所有需要下载的数量
        /// </summary>
        public int DownloadCount { get; private set; }
        /// <summary>
        /// 已经下载完成的大小
        /// </summary>
        public long CompletedDownloadSize { get; private set; }
        /// <summary>
        /// 总共需要下载的大小
        /// </summary>
        public long TotalRequiredDownloadSize { get; private set; }
        public override void Clear()
        {
            CurrentDownloadNode = default;
            CurrentDownloadIndex = 0;
            DownloadCount = 0;
            CompletedDownloadSize = 0;
            TotalRequiredDownloadSize = 0;
        }
        internal QuarkDownloadingEventArgs() { }
        public static QuarkDownloadingEventArgs Create(QuarkDownloadNode node, int currentDownloadIndex, int downloadCount, long completedDownloadSize, long totalRequiredDownloadSize)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkDownloadingEventArgs>();
            eventArgs.CurrentDownloadNode = node;
            eventArgs.CurrentDownloadIndex = currentDownloadIndex;
            eventArgs.DownloadCount = downloadCount;
            eventArgs.CompletedDownloadSize = completedDownloadSize;
            eventArgs.TotalRequiredDownloadSize = totalRequiredDownloadSize;
            return eventArgs;
        }
        public static void Release(QuarkDownloadingEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
