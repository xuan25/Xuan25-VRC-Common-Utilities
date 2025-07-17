
using System;
using Playlist;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist {

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlaylistDisplayLimitController : UdonSharpBehaviour
    {

        public PlaylistIndexer playlistIndexer;
        public Slider slider;
        public TextMeshProUGUI textValue;
        
        public int minValue = 10;
        public int maxValue = 32768;

        private int value = 0;

        public double power = 3;

        void Start()
        {
            value = playlistIndexer.maxDisplayItems;
            slider.value = (float)ValueConvertBack(value);

            OnApply();
        }

        private double ValueConvertBack(int value)
        {
            double range = maxValue - minValue;
            double valueMapped = (value - minValue) / (double)range;
            return Math.Pow(valueMapped, 1.0 / power);
        }

        private int ValueConvert(double value)
        {
            int range = maxValue - minValue;
            double valueMapped = Math.Pow(value, power);
            int newValue = (int)(valueMapped * range) + minValue;
            return newValue;
        }

        public void OnSliderValueChanged()
        {
            value = ValueConvert(slider.value);
            textValue.text = value.ToString();
        }

        public void OnApply()
        {
            playlistIndexer.maxDisplayItems = value;
            playlistIndexer.ReBuildPlaylist();
        }
    }

}
