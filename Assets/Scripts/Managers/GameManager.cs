using UnityEngine;
using DG.Tweening;

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

    public void DoTimeFreeze(float strength, float duration)
    {
        Debug.Log("<color=red> ==> Begin time freeze ==> </Color>");
        Time.timeScale = strength;

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, duration).SetUpdate(true).SetEase(Ease.OutQuad);
        Debug.Log("<color=green> ==> end time freeze ==> </Color>");
    }
}
