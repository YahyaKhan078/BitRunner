using UnityEngine;
using TMPro;

/// <summary>
/// Missions panel display. Lists each mission's description, progress, reward, and done state.
/// Reads <see cref="Missions.Instance"/>. Assign a single multi-line TMP label.
/// </summary>
public class MissionsUI : MonoBehaviour
{
    public TMP_Text listLabel;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var m = Missions.Instance;
        if (m == null || listLabel == null) return;

        var sb = new System.Text.StringBuilder();
        foreach (var mission in m.missions)
        {
            bool done = m.IsComplete(mission);
            int prog = Mathf.Min(m.Progress(mission), mission.target);
            string check = done ? "<color=#39FF6A>[DONE]</color>" : "(" + prog + "/" + mission.target + ")";
            sb.AppendLine(mission.Describe());
            sb.AppendLine("  " + check + "  reward " + mission.reward + "*");
            sb.AppendLine();
        }
        listLabel.text = sb.ToString();
    }
}
