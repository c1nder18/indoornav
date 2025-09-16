using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Immersal.XR;
using Immersal;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ARManager : MonoBehaviour
{
    // This text show current location name in UI (显示当前位置名字)
    public TextMeshProUGUI currentLocationLabel;    // Display current location name
    // This text show how long time between two successful localization (显示两次定位之间的时间)
    public TextMeshProUGUI elapsedTimeLabel;        // Display elapsed time between successful localizations
    // Immersal session object, control AR session (Immersal 会话对象)
    public ImmersalSession session;                 //
    // Localizer do localization event callback (定位回调器)
    public Localizer localizer;                     // Invokes localization events
    // Immersal SDK main object (Immersal SDK 主接口)
    public ImmersalSDK sdk;                         // 
    // This image is green glow show in screen edge (屏幕边缘绿色光晕效果)
    public Image greenGlow;                         // Green glow of the edge of screen
    // Universal Render Data, for URP pipeline (URP 渲染数据)
    public UniversalRendererData urd;

    // Save all localization time here (保存所有定位耗时)
    private List<float> _localizationTimes = new List<float>();
    // Record the max time take to localize (记录最大定位耗时)
    private float _maxLocalizationTime = 0f;
    // Time when start localization (开始定位的时间)
    private float _startTime;   // Time when the first localization started
    // How long between two localization (两次定位的间隔)
    private float _elapsedTime; // Time between successful localizations

    // When location change, update label text (位置改变时更新 UI 文本)
    public void LocationChanged(GameObject newLocation)
    {
        currentLocationLabel.text = newLocation.name;
    }

    // When start localization, remember start time (开始定位时记录时间)
    public void OnLocalizationStart()
    {
        _startTime = Time.time;
    }

    // When first localization success (第一次定位成功时) — here comment out
    public void OnFirstSuccessfulLocalization()
    {
        // _elapsedTime = Time.time - _startTime;
        // elapsedTimeLabel.text = "Čas 1. lokalizace:  " + _elapsedTime.ToString("0.000") + " s";
        // Debug.Log("First successful localization was after: " + _elapsedTime.ToString("0.000") + " s");
    }

    // This call when localization success (定位成功时调用)
    public void OnSuccessfulLocalization(int[] mapIds)
    {
        // calculate time use for this localization (计算这次定位耗时)
        _elapsedTime = Time.time - _startTime;
        // add to list for later calculate average (加入列表计算平均值)
        _localizationTimes.Add(_elapsedTime);

        // update max time if this one is bigger (更新最大耗时)
        if (_elapsedTime > _maxLocalizationTime)
            _maxLocalizationTime = _elapsedTime;

        // calculate average time (计算平均耗时)
        float averageTime = _localizationTimes.Average();

        // Show all time info in text (在 UI 中显示时间信息)
        elapsedTimeLabel.text =
            $"Δt: {_elapsedTime:0.000} s | " +
            $"avg: {averageTime:0.000} s | " +
            $"max: {_maxLocalizationTime:0.000} s";

        // Also print log in console (在控制台输出日志)
        Debug.Log($"Localization: {_elapsedTime:0.000}s | Avg: {averageTime:0.000}s | Max: {_maxLocalizationTime:0.000}s");

        // Start green glow animation (启动绿色光晕动画)
        StartCoroutine(Glow());
        // Reset start time for next localization (重置计时器)
        _startTime = Time.time;
    }

    // This coroutine make green glow fade in/out like breathing (绿色光晕呼吸效果)
    IEnumerator Glow()
    {
        Color initialColor = greenGlow.color;

        float duration = 2f;  // how long fade in or fade out (渐入或渐出时间)
        float time = 0f;

        // fade in (渐入)
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, time / duration);
            greenGlow.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        // fade out (渐出)
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            greenGlow.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        // restore original color (恢复原始颜色)
        greenGlow.color = initialColor;
    }

    // Below function just for debug (下面几个是调试用) — 可以重置会话和 SDK

    // Reset session (重置会话)
    public void OnResetSession()
    {
        session.TriggerResetSession();
    }

    // Reset localizer, also restart session (重置定位器并重启会话)
    public void OnResetLocalizer()
    {
        _ = localizer.StopAndCleanUp();
        session.TriggerResetSession();
    }

    // Restart SDK directly (直接重启 SDK)
    public void OnResetSDK()
    {
        sdk.RestartSdk();
    }
}
