using UnityEngine;
using System;
using System.Collections;

public static class UpdateCoroutines
{
    public static IEnumerator UpdateCoroutine(this MonoBehaviour script, float wait, Action<float> func)
    {
        float lastUpdate = Time.time;
        if (func != null)
        {
            while (true)
            {
                yield return new WaitForSeconds(wait);
                //Debug.Log(Time.time - lastUpdate);
                func(Time.time - lastUpdate);
                lastUpdate = Time.time;
            }
        }
    }

    public static IEnumerator UpdateCoroutine(this MonoBehaviour script, float wait, Action func)
    {
        if (func != null)
        {
            while (true)
            {
                yield return new WaitForSeconds(wait);
                //Debug.Log(Time.time - lastUpdate);
                func();
            }
        }
    }

    public static IEnumerator UpdateCoroutineEndOfFrame(this MonoBehaviour script, float wait, Action<float> func)
    {
        float lastUpdate = Time.time;
        if (func != null)
        {
            while (true)
            {
                yield return new WaitForSeconds(wait);
                yield return new WaitForEndOfFrame();
                //Debug.Log(Time.time - lastUpdate);
                func(Time.time - lastUpdate);
                lastUpdate = Time.time;
            }
        }
    }

    public static IEnumerator UpdateCoroutineEndOfFrame(this MonoBehaviour script, float wait, Action func)
    {
        if (func != null)
        {
            while (true)
            {
                yield return new WaitForSeconds(wait);
                yield return new WaitForEndOfFrame();
                //Debug.Log(Time.time - lastUpdate);
                func();
            }
        }
    }
}
