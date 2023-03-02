using Quark.Recyclable;
using System;
using System.Collections.Generic;

namespace Quark
{
    internal class QuarkEventArgsPool
    {
        static readonly Dictionary<Type, Pool<QuarkEventArgsBase>> poolDict
    = new Dictionary<Type, Pool<QuarkEventArgsBase>>();
        public static T Acquire<T>() where T :  QuarkEventArgsBase
        {
            return GetPool(typeof(T)).Spawn() as T;
        }
        public static void Release<T>(T eventArgs)
            where T : QuarkEventArgsBase
        {
            var type = typeof(T);
            GetPool(type).Despawn(eventArgs);
        }
        static Pool<QuarkEventArgsBase> GetPool(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("Type is invalid !");
            if (!poolDict.TryGetValue(type, out var pool))
            {
                pool = new Pool<QuarkEventArgsBase>(() => { return Activator.CreateInstance(type) as QuarkEventArgsBase; }, (t) => { t.Clear(); });
                poolDict.Add(type, pool);
            }
            return pool;
        }
    }
}
