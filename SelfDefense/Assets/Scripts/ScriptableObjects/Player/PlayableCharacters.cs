using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ScriptableObjects.Player
{
    [Serializable]
    public struct CharacterSpriteData
    {
        [SerializeField]
        public List<Sprite> Idle;
    }
    
    [CreateAssetMenu(fileName = "PlayableCharacters", menuName = "MultiplayerSAT/PlayableCharacters", order = 0)]
    public class PlayableCharacters : ScriptableObject
    {
        public List<CharacterSpriteData> Sprites;

        public List<Sprite> Goals;
    }
}