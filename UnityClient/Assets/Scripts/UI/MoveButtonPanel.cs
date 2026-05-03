using System;
using System.Collections.Generic;
using UnityEngine;

// Holds the 4 spell card slots and populates them from the hero's move list.
public class MoveButtonPanel : MonoBehaviour
{
    [SerializeField] List<MoveButton> buttons = new();

    public void Populate(List<string> moveIds, Dictionary<string, MoveData> allMoves, Action<string> onMoveClicked)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < moveIds.Count && allMoves.TryGetValue(moveIds[i], out MoveData move))
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].Setup(move, onMoveClicked);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetInteractable(bool on)
    {
        foreach (var btn in buttons)
            btn.SetInteractable(on);
    }

    public void RefreshCooldowns(Dictionary<string, int> cooldowns)
    {
        foreach (var btn in buttons)
        {
            if (!btn.gameObject.activeSelf) continue;
            cooldowns.TryGetValue(btn.MoveId, out int turns);
            btn.SetCooldown(turns);
        }
    }
}
