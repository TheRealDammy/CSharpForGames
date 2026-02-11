using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    public Room CurrentRoom { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RoomTrigger trigger = other.GetComponent<RoomTrigger>();
        if (trigger == null) return;

        CurrentRoom = trigger.RoomReference;

        if (!CurrentRoom.HasBeenDiscovered)
        {
            CurrentRoom.HasBeenDiscovered = true;
            CheckpointManager.Instance.SetCheckpointRoom(CurrentRoom);
        }
    }
}
