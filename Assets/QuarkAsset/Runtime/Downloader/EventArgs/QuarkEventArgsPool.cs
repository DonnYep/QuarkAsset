using Quark.Recyclable;
using System;
using System.Collections.Generic;

namespace Quark
{
    internal class QuarkEventArgsPool
    {
        static readonly Dictionary<Type, QuarkPool<QuarkEventArgsBase>> poolDict
    = new Dictionary<Type, QuarkPool<QuarkEventArgsBase>>();
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
        static QuarkPool<QuarkEventArgsBase> GetPool(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("Type is invalid !");
            if (!poolDict.TryGetValue(type, out var pool))
            {
                pool = new QuarkPool<QuarkEventArgsBase>(() => { return Activator.CreateInstance(type) as QuarkEventArgsBase; }, (t) => { t.Clear(); });
                poolDict.Add(type, pool);
            }
            return pool;
        }
    }
}
