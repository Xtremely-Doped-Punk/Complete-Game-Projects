using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class PlayerIconSingleUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI iconText;
        public ulong ClientID { get; private set; }
        public bool Vote { get; private set; }

        public void SetupPlayerIconVote(KeyValuePair<ulong,bool> pair)
        {
            this.ClientID = pair.Key;
            this.Vote = pair.Value;
            iconText.text = $"ID:{ClientID}";
        }

        public void ResetPlayerIconVote(bool defaultVote)
        {
            this.Vote = defaultVote;
            iconImage.color = Color.white;
        }
        
        public void UpdatePlayerIconVote(bool ready)
        {
            this.Vote = ready;
            UpdatePlayerIconVote();
        }
        public void UpdatePlayerIconVote()
        {
            iconImage.color = Vote ? Color.green : Color.red;
        }
    }
}