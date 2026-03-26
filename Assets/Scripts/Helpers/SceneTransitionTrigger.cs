using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneTransistor _sceneTransistor;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            _sceneTransistor.LoadScene();
        }
    }
}
