using UnityEngine;
using System.Collections;

public static class HitStop
{
    public static IEnumerator Stop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
