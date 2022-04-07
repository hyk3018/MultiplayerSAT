using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects.Player;
using UnityEngine;

namespace Client.UI
{
    public class CharacterCustomizationUI : MonoBehaviour
    {
        [SerializeField]
        private PlayableCharacters playableCharacters;

        [SerializeField]
        private SpriteSelectionUI characterSelector;
        
        [SerializeField]
        private SpriteSelectionUI goalSelector;

        private void Start()
        {
            List<Sprite> characters = playableCharacters.Sprites
                .Select(character => character.Idle[0]).ToList();
            characterSelector.Initialise(characters);
            goalSelector.Initialise(playableCharacters.Goals);
        }
    }
}