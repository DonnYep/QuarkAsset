namespace Quark
{
    internal interface IQuarkTask
    {
        long TaskId { get; }
        bool IsCompleted { get; set; }
        void OnLoadDone(UnityEngine.Object asset);
    }
}
