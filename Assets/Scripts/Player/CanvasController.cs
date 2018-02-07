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
        
        public void SetCurrencyText(string text)
        {
            CurrencyText.text = text;
        }

        public void SetHealthBar(float calcHealth)
        {
            healthBar.transform.localScale = new Vector3(Mathf.Clamp(calcHealth, 0f, 1f), healthBar.transform.localScale.y, healthBar.transform.localScale.z);            
        }



    }
}
