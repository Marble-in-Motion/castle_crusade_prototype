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
        private RenderTexture[] renderTextures;

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

		[SerializeField]
		private GameObject[] sectors; 

        private GameObject GetSector(int teamId, int laneId)
        {
            return (teamId == TeamController.TEAM1)
            ? sectors[laneId]
            : sectors[laneId + 5];
        }

		public void HighlightSector(int teamId, int laneId) {

            GameObject sectorprefab = GetSector(teamId, laneId);
			Instantiate(sectorprefab, sectorprefab.transform.position, sectorprefab.transform.rotation);
            
		}
		        
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

        public void SetRenderTexture(int teamId)
        {
            // teamId 1 (blue) -> index 0 (tower 2)
            // teamId 2 (red) -> index 1 (tower 1)
            miniMapView.GetComponent<RawImage>().texture = renderTextures[teamId - 1];
        }

        public void SetGameOverValue(TeamController.TeamResult teamResult)
        {
            switch (teamResult)
            {
                case TeamController.TeamResult.LOST:
                    anim.ResetTrigger("Restart");
                    anim.SetTrigger("GameOver");
					anim.SetTrigger("ResetSendTroopAlert");
                    break;
                case TeamController.TeamResult.WON:
                    anim.ResetTrigger("Restart");
                    anim.SetTrigger("GameWin");
					anim.SetTrigger("ResetSendTroopAlert");
				    break;
                case TeamController.TeamResult.UNDECIDED:
                    anim.ResetTrigger("GameWin");
                    anim.ResetTrigger("GameOver");
                    anim.SetTrigger("Restart");
                    break;
            }
        }

		public void SetArrowCooldown()
        {
			anim.SetTrigger("Cooldown");
		}

        public void SetSendTroopAlert()
        {
            anim.ResetTrigger("ResetSendTroopAlert");
            anim.SetTrigger("SendTroopAlert");
        }

        public void ResetSendTroopAlert()
        {
            anim.ResetTrigger("SendTroopAlert");
            anim.SetTrigger("ResetSendTroopAlert");
        }

        public void SetSandboxAlert()
        {
			anim.ResetTrigger("GameWin");
			anim.ResetTrigger("GameOver");
			anim.SetTrigger("Restart");

			anim.ResetTrigger("ResetSandbox");
            anim.SetTrigger("Sandbox");
        }

        public void ResetSandboxAlert()
        {
            anim.ResetTrigger("Sandbox");
            anim.SetTrigger("ResetSandbox");
        }

        public void SetSpendGold()
        {
            anim.SetTrigger("DeductGold");
        }

    }
}
