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
                .Select(character => character.Right).ToList();
            characterSelector.Initialise(characters, null);
            goalSelector.Initialise(playableCharacters.Goals, playableCharacters.GoalNames);
        }
    }
}