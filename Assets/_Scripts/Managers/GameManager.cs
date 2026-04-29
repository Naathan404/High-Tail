using UnityEngine;
using DG.Tweening;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }


    /// <summary>
    /// Za warudo
    /// </summary>
    /// <param name="strength"></param>
    /// <param name="duration"></param>
    public void DoTimeFreeze(float strength, float duration)
    {
        Debug.Log("<color=red> ==> Begin time freeze ==> </Color>");
        Time.timeScale = strength;

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, duration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                Debug.Log("<color=green> ==> end time freeze ==> </Color>");
            }); 
    }
    
    public void StopTime(float duration)
    {
        StartCoroutine(HitStop(duration));
    }

    private IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }
}
