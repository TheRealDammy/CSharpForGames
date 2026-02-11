using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated;
    private bool cleared;
    private List<GameObject> guards = new List<GameObject>();

    public void RegisterGuard(GameObject enemy)
    {
        guards.Add(enemy);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            Activate();
        }
    }

    private void Activate()
    {
        activated = true;

        RespawnManager.Instance.SetCheckpoint(transform.position);

        Debug.Log("Checkpoint Activated");

        GetComponent<SpriteRenderer>().color = Color.green;
    }
}
