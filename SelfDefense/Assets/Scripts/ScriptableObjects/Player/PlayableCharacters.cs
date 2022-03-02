using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Player
{
    [Serializable]
    public struct CharacterSpriteData
    {
        [SerializeField]
        public Sprite Left;
        
        [SerializeField]
        public Sprite Right;
    }
    
    [CreateAssetMenu(fileName = "PlayableCharacters", menuName = "MultiplayerSAT/PlayableCharacters", order = 0)]
    public class PlayableCharacters : ScriptableObject
    {
        public List<CharacterSpriteData> Sprites;
    }
}