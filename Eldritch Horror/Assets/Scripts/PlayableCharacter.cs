using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
public class PlayableCharacter : Entity
{
    [SerializeField, TabGroup("References")] private SpriteRenderer _highlightSprite;

    public void SetSelectionState(int state)
    {
        switch (state)
        {
            case 0: //default
                _highlightSprite.color = Color.black;
                break;
            case 1: //mouse entered
                _highlightSprite.color = Color.gray;
                break;
            case 2: //active
                _highlightSprite.color = Color.white;
                break;
        }
    }
}
