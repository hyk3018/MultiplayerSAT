using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects.Player
{
    [Serializable]
    public struct CharacterSpriteData
    {
        [FormerlySerializedAs("Idle")]
        [SerializeField]
        public Sprite Right;

        [SerializeField]
        public Sprite Left;
    }
    
    [CreateAssetMenu(fileName = "PlayableCharacters", menuName = "MultiplayerSAT/PlayableCharacters", order = 0)]
    public class PlayableCharacters : ScriptableObject
    {
        public List<CharacterSpriteData> Sprites;

        public List<Sprite> Goals;

        public List<string> GoalNames;
    }
}