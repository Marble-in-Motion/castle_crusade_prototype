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
        private Image blueHealthBar;

		[SerializeField]
		private Image redHealthBar;

        [SerializeField]
        private Text CurrencyText;

        [SerializeField]
        private Animator anim;

        [SerializeField]
        private GameObject miniMapView;
		        
        public void SetCurrencyText(string text)
        {
            CurrencyText.text = text;
        }

        public void SetBlueHealthBar(float health)
        {
            blueHealthBar.transform.localScale = new Vector3(Mathf.Clamp(health, 0f, 1f), blueHealthBar.transform.localScale.y, blueHealthBar.transform.localScale.z);
        }

		public void SetRedHealthBar(float health) {
            redHealthBar.transform.localScale = new Vector3(Mathf.Clamp(health, 0f, 1f), redHealthBar.transform.localScale.y, redHealthBar.transform.localScale.z);
		}

        public void SetRenderTexture(RenderTexture texture)
        {
            miniMapView.GetComponent<RawImage>().texture = texture;
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

		public void SetArrowCooldown() {
			anim.SetTrigger ("Cooldown");
		}

    }
}
