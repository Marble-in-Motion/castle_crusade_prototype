using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Player
{
    /**
     * UI canvas controller for a single player
     **/
    public class CanvasController : MonoBehaviour
    {
        private bool displayTime = false;

        [SerializeField]
        private RenderTexture[] renderTextures;

        [SerializeField]
        private Image blueHealthBar;

        [SerializeField]
        private Image redHealthBar;

        [SerializeField]
        private Text CurrencyText;

        [SerializeField]
        private Text LeaderboardText;

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

        public void HighlightSector(int teamId, int laneId)
        {

            GameObject sectorprefab = GetSector(teamId, laneId);
            Instantiate(sectorprefab, sectorprefab.transform.position, sectorprefab.transform.rotation);

        }

        public void SetCurrencyText(string text)
        {
            CurrencyText.text = text;
        }

        public void SetLeaderboardText(string text, bool isOpponentAiEnabled, bool teamAiEnabled)
        {
            LeaderboardText.text = text;
            LeaderboardText.enabled = (!teamAiEnabled && isOpponentAiEnabled);

        }

        public void SetBlueHealthBar(float health)
        {
            blueHealthBar.transform.localScale = new Vector3(Mathf.Clamp(health, 0f, 1f), blueHealthBar.transform.localScale.y, blueHealthBar.transform.localScale.z);
        }

        public void SetRedHealthBar(float health)
        {
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
                    if (!displayTime)
                    {
                        SetAITimerAlert();
                    }
                    displayTime = true;
                    break;
                case TeamController.TeamResult.WON:
                    anim.ResetTrigger("Restart");
                    anim.SetTrigger("GameWin");
                    anim.SetTrigger("ResetSendTroopAlert");
                    if (!displayTime)
                    {
                        SetAITimerAlert();
                    }
                    displayTime = true;
                    break;
                case TeamController.TeamResult.UNDECIDED:
                    anim.ResetTrigger("GameWin");
                    anim.ResetTrigger("GameOver");
                    anim.SetTrigger("Restart");
                    displayTime = false;
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

        public void ResetAITimerAlert()
        {
            anim.Play("Start_AI", -1, 0f);
        }

        public void SetAITimerAlert()
        {
            anim.Play("SetAITimer", -1, 0f);
        }

        public void SetSpendGold()
        {
            anim.SetTrigger("DeductGold");
        }

        public void ResetVolleyAnim()
        {
            anim.Play("IdleVolley", -1, 0f);
        }

    }
}
