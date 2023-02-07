using Quark.Recyclable;
using System;
using System.Collections.Generic;

namespace Quark
{
    public class QuarkPool
    {
        static readonly Dictionary<Type, Pool<IRecyclable>> poolDict
    = new Dictionary<Type, Pool<IRecyclable>>();
        public static T Acquire<T>() where T : class, IRecyclable, new()
        {
            return GetPool(typeof(T)).Spawn() as T;
        }
        public static void Release(IRecyclable recyclable)
        {
            var type = recyclable.GetType();
            GetPool(type).Despawn(recyclable);
        }
        static Pool<IRecyclable> GetPool(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("Type is invalid !");
            if (!poolDict.TryGetValue(type, out var pool))
            {
                pool = new Pool<IRecyclable>(() => { return Activator.CreateInstance(type) as IRecyclable; }, (t) => { t.Clear(); });
                poolDict.Add(type, pool);
            }
            return pool;
        }
    }
}
