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
		private GameObject attackBar;

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

		public void SetSpartanDistances(Dictionary<string, float> troopLocs)
        {
			//Go through and update sprite locations or delete them
			List<string> spritesToDelete = new List<string>();
			foreach(KeyValuePair<string, Image> sprite in troopSprites)
			{
				if (troopLocs.ContainsKey(sprite.Key)) 
				{
					Image spriteImg = troopSprites[sprite.Key];
					RectTransform rt = (RectTransform) attackBar.transform;
					float width = rt.rect.width;

					float ratio = troopLocs [sprite.Key];

					float xLoc = (troop.transform.localPosition.x - width / 2) + (ratio * width);
					spriteImg.transform.localPosition = new Vector3(xLoc, spriteImg.transform.localPosition.y, spriteImg.transform.localPosition.z);
					troopLocs.Remove (sprite.Key);
				} else
				{
					spritesToDelete.Add (sprite.Key);
				} 
			}
			//Remove sprite locations from dictionary
			foreach(string sprite in spritesToDelete) {
				DestroySpartanSprite (sprite);
			}
			//Initialise new sprites
			foreach (KeyValuePair<string, float> troopLoc in troopLocs) {
				Image spriteImg = Instantiate(troop, attackBar.transform) as Image;
				troopSprites.Add(troopLoc.Key, spriteImg);
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
