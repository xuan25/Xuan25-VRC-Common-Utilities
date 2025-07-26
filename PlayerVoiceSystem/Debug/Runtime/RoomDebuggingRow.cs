
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace Xuan25.PlayerVoiceSystem.Debugging
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RoomDebuggingRow : UdonSharpBehaviour
    {
        public TextMeshProUGUI textUsername;

        public GameObject indicatorTrue;
        public GameObject indicatorFalse;

        public GameObject indicatorContainer;

        // interleaved indicators for true and false states
        private GameObject[] indicators;

        private int roomCountMax;

        void Start()
        {

        }

        public void Setup(int roomCountMax)
        {
            this.roomCountMax = roomCountMax;

            if (indicators != null)
            {
                foreach (GameObject indicator in indicators)
                {
                    Destroy(indicator);
                }
            }

            indicators = new GameObject[roomCountMax * 2];

            for (int i = 0; i < roomCountMax; i++)
            {
                indicators[2 * i] = Instantiate(indicatorTrue, indicatorContainer.transform);
                indicators[2 * i + 1] = Instantiate(indicatorFalse, indicatorContainer.transform);
            }
        }

        public void SetUserName(string username)
        {
            textUsername.text = username;
        }

        public void SetMask(int mask, int roomValidityMask)
        {
            // Debug.Log($"[{GetUdonTypeName()}] SetMask called with mask: {mask}, roomValidityMask: {roomValidityMask}; Indicators count: {indicators.Length}");
            for (int i = 0; i < roomCountMax; i++)
            {
                bool isValidRoom = (roomValidityMask & (1 << i)) != 0;
                bool flag = (mask & (1 << i)) != 0;

                if (isValidRoom)
                {
                    indicators[2 * i].SetActive(flag);
                    indicators[2 * i + 1].SetActive(!flag);
                }
                else
                {
                    indicators[2 * i].SetActive(false);
                    indicators[2 * i + 1].SetActive(false);
                }
            }
        }
    }

}