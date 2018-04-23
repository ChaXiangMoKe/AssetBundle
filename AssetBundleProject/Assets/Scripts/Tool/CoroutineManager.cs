using UnityEngine;
using System;
using System.Collections;

public class CoroutineManager : MonoBehaviour {

    public static CoroutineManager Instace { get; private set; }

    public delegate bool Condition();
	// Use this for initialization
	void Awake () {
        Instace = this;
	}

    void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    public Coroutine WaitAction(Condition condition,Action action)
    {
        return StartCoroutine(DoWaitConditionAction(condition,action));
    }

    private IEnumerator DoWaitConditionAction(Condition condition,Action action)
    {
        while (!condition.Invoke())
            yield return 0;

        action();
    }

    private IEnumerator DoWaitConditionAction(Action action)
    {
        yield return new WaitForEndOfFrame();
        action();
    }

    private IEnumerator DoWaitConditionAction(Action action,float second)
    {
        yield return new WaitForSeconds(second);
        action();
    }

    private void Update()
    {
    }
}
