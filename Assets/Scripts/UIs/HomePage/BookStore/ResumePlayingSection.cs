using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using EventDispatcher;
using DataCore;

public class ResumePlayingSection : MonoBehaviour
{
    [SerializeField] private ResumePlayingTab tabPrefab;
    [SerializeField] private Transform tabContainer;
    [SerializeField] private Transform footer;

    private MasterDataStore _masterDataStore;
    private List<ResumePlayingTab> _tabs;

    public void Init()
    {
        _masterDataStore = MasterDataStore.Instance;
        _tabs = new List<ResumePlayingTab>();
        UpdateData();
        this.RegisterListener(EventID.OnUpdateResumePlaying, (o) =>
        {
            this.UpdateData();
        });
    }

    public void UpdateData()
    {
        DataCore.Debug.Log("ResumePlayingSection UpdateData", false);
        List<LastPuzzlePlay> lastPlays = GameData.Instance.SavedPack.LastPuzzlePlays;
        if (lastPlays.Count > 0)
        {
            gameObject.SetActive(true);

            int amount = lastPlays.Count;
            PuzzleLevelData puzzleData;

            CheckTabs(amount);

            //DataCore.Debug.Log(_tabs.Count + "=== " + amount);

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (i < amount)
                {
                    _tabs[i].gameObject.SetActive(true);
                    puzzleData = _masterDataStore.GetPuzzleById(lastPlays[lastPlays.Count - i - 1].BookId,
                        lastPlays[lastPlays.Count - i - 1].PartId, lastPlays[lastPlays.Count - i - 1].PuzzleId);
                    if (puzzleData)
                    {
                        _tabs[i].SetData(lastPlays[lastPlays.Count - i - 1], puzzleData);
                    }
                    else
                    {
                        _tabs[i].gameObject.SetActive(false);
                    }

                }
                else
                {
                    _tabs[i].gameObject.SetActive(false);
                }
            }

            DOVirtual.DelayedCall(0.0f, () =>
            {
                footer.SetAsLastSibling();
            }).Play();
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    private void CheckTabs(int amount)
    {
        if (_tabs == null) return;
        if (tabPrefab == null || tabContainer == null) return;
        int newTabAmount = amount - _tabs.Count;

        if (newTabAmount > 0)
        {
            ResumePlayingTab newTab;
            for (int i = 0; i < newTabAmount; i++)
            {
                newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
                _tabs.Add(newTab);
            }
        }
    }
}
