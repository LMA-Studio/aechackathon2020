using System.Collections;
using UnityEngine;

namespace LMAStudio.StreamVR.Unity.Helpers
{
    public class CoroutineWithData<T> where T: class
    {
        public Coroutine coroutine { get; private set; }
        public T result;
        private IEnumerator target;

        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current as T;
                yield return result;
            }
        }
    }
}
