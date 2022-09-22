namespace Quark
{
    public interface IQuarkTask
    {
        long TaskId { get; }
        bool IsCompleted { get; set; }
        void OnLoadDone(UnityEngine.Object asset);
    }
}
