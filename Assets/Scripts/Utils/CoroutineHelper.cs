using UnityEngine;
using System.Collections;

namespace CoroutineHelper
{
    public class CoroutineWithData
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator _target;
        public CoroutineWithData(MonoBehaviour owner, IEnumerator target_)
        {
            _target = target_;
            coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (_target.MoveNext())
            {
                result = _target.Current;
                yield return result;
            }
        }
    }
}
