using System.Collections;
using UnityEngine;
using System.Threading;
using System;

public static class MonoBehaviourExtensions
{
    static IEnumerator CancellableCoroutine(IEnumerator routine, CancellationToken token)
    {
        if (routine == null)
            throw new ArgumentNullException(nameof(routine));

        if (token == CancellationToken.None)
            throw new ArgumentException("Invalid argument: CancellationToken.None", nameof(token));

        while (!token.IsCancellationRequested && routine.MoveNext())
        {
            yield return routine.Current;
        }
    }

    public static Coroutine StartCoroutine(this MonoBehaviour monoBehaviour, IEnumerator routine, CancellationToken cancellationToken)
    {
        return monoBehaviour.StartCoroutine(CancellableCoroutine(routine, cancellationToken));
    }
}
