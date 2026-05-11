using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string NPCID { get; private set; }
    public DialogueData[] dialogueData;
    public GameObject interactIcon;
    public int dialogueIndex = 0;

    PlayerController player;
    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            if(player.transform.position.x < transform.position.x)
            {
                this.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                this.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
