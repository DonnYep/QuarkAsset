using System;
using System.Collections.Generic;

namespace Quark
{
    public class QuarkPool<T>
 where T : class
    {
        public int Count { get { return objects.Count; } }
        readonly Queue<T> objects = new Queue<T>();
        readonly Func<T> objectGenerator;
        readonly Action<T> objectDespawn;
        public QuarkPool(Func<T> objectGenerator, Action<T> objectDespawn)
        {
            this.objectGenerator = objectGenerator;
            this.objectDespawn = objectDespawn;
        }
        public T Spawn()
        {
            if (objects.Count > 0)
            {
                var obj = objects.Dequeue();
                return obj;
            }
            else
            {
                var obj = objectGenerator();
                return obj;
            }
        }
        public void Despawn(T obj)
        {
            objectDespawn?.Invoke(obj);
            objects.Enqueue(obj);
        }
        public void Clear()
        {
            objects.Clear();
        }
    }
}
