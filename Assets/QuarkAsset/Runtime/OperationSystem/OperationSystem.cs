using System.Collections.Generic;
using System.Diagnostics;
namespace Quark
{
    internal class OperationSystem
    {
        private static readonly List<AsyncOperationBase> operations = new List<AsyncOperationBase>(1000);
        private static readonly List<AsyncOperationBase> newList = new List<AsyncOperationBase>(1000);
        // 计时器相关
        private static Stopwatch _watch;
        private static long frameTime;
        /// <summary>
        /// 异步操作的最小时间片段
        /// </summary>
        public static long MaxTimeSlice { set; get; } = long.MaxValue;
        /// <summary>
        /// 处理器是否繁忙
        /// </summary>
        public static bool IsBusy
        {
            get
            {
                return _watch.ElapsedMilliseconds - frameTime >= MaxTimeSlice;
            }
        }

        /// <summary>
        /// 初始化异步操作系统
        /// </summary>
        public static void Initialize()
        {
            _watch = Stopwatch.StartNew();
        }
        public static void Update()
        {
            // 添加新增的异步操作
            if (newList.Count > 0)
            {
                bool sorting = false;
                foreach (var operation in newList)
                {
                    if (operation.Priority > 0)
                    {
                        sorting = true;
                        break;
                    }
                }

                operations.AddRange(newList);
                newList.Clear();

                // 重新排序优先级
                if (sorting)
                    operations.Sort();
            }
            // 更新进行中的异步操作
            for (int i = 0; i < operations.Count; i++)
            {
                if (IsBusy)
                    break;

                var operation = operations[i];
                if (operation.IsFinish)
                    continue;

                if (operation.IsDone == false)
                    operation.OnUpdate();

                if (operation.IsDone)
                    operation.SetFinish();
            }
            // 移除已经完成的异步操作
            for (int i = operations.Count - 1; i >= 0; i--)
            {
                var operation = operations[i];
                if (operation.IsFinish)
                    operations.RemoveAt(i);
            }
        }
        /// <summary>
        /// 销毁异步操作系统
        /// </summary>
        public static void DestroyAll()
        {
            operations.Clear();
            newList.Clear();
            _watch = null;
            frameTime = 0;
            MaxTimeSlice = long.MaxValue;
        }
        /// <summary>
        /// 开始处理异步操作类
        /// </summary>
        public static void StartOperation(AsyncOperationBase operation)
        {
            newList.Add(operation);
            operation.SetStart();
        }
    }
}