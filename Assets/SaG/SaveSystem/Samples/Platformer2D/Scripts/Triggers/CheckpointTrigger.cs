﻿using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D.Triggers
{
    public class CheckpointTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GAME.CheckpointSave();
            }
        }
    }
}
