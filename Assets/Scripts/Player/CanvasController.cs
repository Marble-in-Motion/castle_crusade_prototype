using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    public class CanvasController : MonoBehaviour
    {

        [SerializeField]
        private Image healthBar;

        [SerializeField]
        private Text CurrencyText;

        [SerializeField]
        private Animator anim;

        [SerializeField]
        private Image troop;

        private Dictionary<String, Image> troopSprites = new Dictionary<string, Image>();
        
        public void SetCurrencyText(string text)
        {
            CurrencyText.text = text;
        }

        public void SetHealthBar(float calcHealth)
        {
            healthBar.transform.localScale = new Vector3(Mathf.Clamp(calcHealth, 0f, 1f), healthBar.transform.localScale.y, healthBar.transform.localScale.z);            
        }

        public void SetGameOverValue(GameController.GameState gameOverValue)
        {
            switch (gameOverValue)
            {
                case GameController.GameState.GAME_LOST:
                    anim.ResetTrigger("Restart");
                    anim.SetTrigger("GameOver");
                    break;
                case GameController.GameState.GAME_WON:
                    anim.ResetTrigger("Restart");
                    anim.SetTrigger("GameWin");
                    break;
                case GameController.GameState.GAME_RESTART:
                    anim.ResetTrigger("GameWin");
                    anim.ResetTrigger("GameOver");
                    anim.SetTrigger("Restart");
                    break;
            }
        }

        public void SetSpartanDistance(string idTag, float ratio)
        {
            if (troopSprites.ContainsKey(idTag))
            {
                Image sprite = troopSprites[idTag];
                Vector3 newLocaction = new Vector3(130 - (260 * ratio), sprite.transform.localPosition.y, sprite.transform.localPosition.z);
                sprite.transform.localPosition = newLocaction;
            } else
            {
                Image sprite = Instantiate(troop, transform) as Image;
                troopSprites.Add(idTag, sprite);
            } 
        }

        public void DestroySpartanSprite(string idTag)
        {
            if (troopSprites.ContainsKey(idTag))
            {
                Image sprite = troopSprites[idTag];
                Destroy(sprite);
                troopSprites.Remove(idTag);
            }
        }

    }
}
