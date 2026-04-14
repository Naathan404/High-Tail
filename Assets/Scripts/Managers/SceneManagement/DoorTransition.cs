using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    
    [Header("Door's Identity")]
    public string DoorID;
    public Transform SpawnPoint; // Vị trí thả Sóc Kite xuống nếu đi TỪ phòng khác đến cửa này

    [Header("Transition Settings")]
    public string SceneToLoad; // Tên phòng sắp tới (VD: "Room_B")
    public string TargetDoorID; // Mã cửa đích ở phòng tới (VD: "Door_Left_01")

    private PlayerController _player;

    private void Awake()
    {
        try
        {
            _player = FindAnyObjectByType<PlayerController>();
        }
        catch(Exception ex)
        {
            Debug.Log($"không tìm thấy player. lỗi {ex}");
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            InputManager.Instance.DisableControl();
            
            // call SceneTransitionManager to load new scene async with fading anim
            SceneTransitionManager.Instance.DoTransition(gameObject.scene.name, SceneToLoad, TargetDoorID);
        }
    }
}