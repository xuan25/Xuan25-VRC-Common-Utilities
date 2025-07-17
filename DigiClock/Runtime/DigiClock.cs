
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DigiClock : UdonSharpBehaviour
{
    public TMPro.TextMeshProUGUI dateText;
    public TMPro.TextMeshProUGUI timeText;
    public TMPro.TextMeshProUGUI dayText;

    private float updateInterval = 60f; // 更新間隔（秒）
    private float dayOfWeekUpdateInterval = 60f; // 曜日の更新間隔（秒）

    private void Start()
    {
        UpdateDateTime();
        UpdateDayOfWeek();
    }

    public void UpdateDateTime()
    {
        System.DateTime now = System.DateTime.Now;
        dateText.text = now.ToString("yyyy/MM/dd");
        timeText.text = now.ToString("HH:mm");
        SendCustomEventDelayedSeconds(nameof(UpdateDateTime), updateInterval);
    }

    public void UpdateDayOfWeek()
    {
        System.DateTime now = System.DateTime.Now;
        dayText.text = now.ToString("dddd", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        SendCustomEventDelayedSeconds(nameof(UpdateDayOfWeek), dayOfWeekUpdateInterval);
    }
}
