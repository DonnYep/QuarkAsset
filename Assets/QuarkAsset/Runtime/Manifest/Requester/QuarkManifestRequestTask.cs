using Quark.Asset;
using System;

namespace Quark.Manifest
{
    internal class QuarkManifestRequestTask : IEquatable<QuarkManifestRequestTask>
    {
        public int TaskId { get; private set; }
        public string Url { get; private set; }
        public byte[] AesKeyBytes { get; private set; }
        public Action<QuarkManifest> OnSuccess { get; private set; }
        public Action<string> OnFailure { get; private set; }
        public QuarkManifestRequestTask(int taskId, string url, byte[] aesKeyBytes, Action<QuarkManifest> onSuccess, Action<string> onFailure)
        {
            TaskId = taskId;
            Url = url;
            AesKeyBytes = aesKeyBytes;
            this.OnSuccess = onSuccess;
            this.OnFailure = onFailure;
        }
        public bool Equals(QuarkManifestRequestTask other)
        {
            return this.TaskId == other.TaskId && this.Url == other.Url;
        }
        public override string ToString()
        {
            return $"TaskId: {TaskId}; URI: {Url}";
        }
    }
}
