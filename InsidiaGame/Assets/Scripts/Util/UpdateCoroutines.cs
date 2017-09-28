using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// These exist to save on a little bit of repetitive coding: having a coroutine that runs a certain amount of times per second.<para></para>
/// To use: In any MonoBehaviour script call "this.UpdateCoroutine(callsPerSecond, function_to_be_called)" The function can either have no parameters or a float paramater for deltaTime.<para></para>
/// Created by: Christian Clark
/// </summary>
public static class UpdateCoroutines
{
    public static IEnumerator UpdateCoroutine(this MonoBehaviour script, float callsPerSecond, Action<float> func)
    {
        float wait = 1f / callsPerSecond;
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

    public static IEnumerator UpdateCoroutine(this MonoBehaviour script, float callsPerSecond, Action func)
    {
        float wait = 1f / callsPerSecond;
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
}
