using System;
using System.Collections;
using UnityEngine;
namespace Quark
{
    public partial class QuarkUtility
    {
        public static class Unity
        {
            #region Coroutine
            public class CoroutineProvider : MonoBehaviour
            {
                public Coroutine StartCoroutine(Action handler)
                {
                    return StartCoroutine(EnumCoroutine(handler));
                }
                public Coroutine PredicateCoroutine(Func<bool> handler, Action callBack)
                {
                    return StartCoroutine(EnumPredicateCoroutine(handler, callBack));
                }
                public Coroutine StartCoroutine(Coroutine routine, Action callBack)
                {
                    return StartCoroutine(EnumCoroutine(routine, callBack));
                }
                public Coroutine StartCoroutine(Action handler, Action callback)
                {
                    return StartCoroutine(EnumCoroutine(handler, callback));
                }
                void Awake()
                {
                    this.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    DontDestroyOnLoad(gameObject);
                }
                IEnumerator EnumCoroutine(Action handler)
                {
                    handler?.Invoke();
                    yield return null;
                }
                IEnumerator EnumCoroutine(Coroutine routine, Action callBack)
                {
                    yield return routine;
                    callBack?.Invoke();
                }
                IEnumerator EnumCoroutine(Action handler, Action callack)
                {
                    yield return StartCoroutine(handler);
                    callack?.Invoke();
                }
                IEnumerator EnumPredicateCoroutine(Func<bool> handler, Action callBack)
                {
                    yield return new WaitUntil(handler);
                    callBack();
                }
            }
            static CoroutineProvider coroutineProvider;
            static CoroutineProvider CoroutineProviderComp
            {
                get
                {
                    if (coroutineProvider == null)
                    {
                        var go = new GameObject("QuarkCoroutinePorvider");
                        coroutineProvider = go.AddComponent<CoroutineProvider>();
                    }
                    return coroutineProvider;
                }
            }
            public static Coroutine StartCoroutine(IEnumerator routine)
            {
                return CoroutineProviderComp.StartCoroutine(routine);
            }
            public static Coroutine StartCoroutine(Action handler)
            {
                return CoroutineProviderComp.StartCoroutine(handler);
            }
            public static Coroutine PredicateCoroutine(Func<bool> handler, Action callBack)
            {
                return CoroutineProviderComp.PredicateCoroutine(handler, callBack);
            }
            public static Coroutine StartCoroutine(Coroutine routine, Action callBack)
            {
                return CoroutineProviderComp.StartCoroutine(routine, callBack);
            }
            public static Coroutine StartCoroutine(Action handler, Action callback)
            {
                return CoroutineProviderComp.StartCoroutine(handler, callback);
            }
            public static void StopCoroutine(IEnumerator routine)
            {
                CoroutineProviderComp.StopCoroutine(routine);
            }
            public static void StopCoroutine(Coroutine routine)
            {
                CoroutineProviderComp.StopCoroutine(routine);
            }
            #endregion
        }
    }
}
