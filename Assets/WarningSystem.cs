using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WarningSystem : MonoBehaviour
{
    EditorScript editor;
    public List<WarningItem> items = new List<WarningItem>();

    private void Awake()
    {
        editor = Camera.main.GetComponent<EditorScript>();
    }

    public void Refresh()
    {
        return;
        items.Clear();
        BeatCubeClass prevCls = null;
        List<BeatCubeClass> clses = editor.project.beatCubeList.OrderBy(c => c.time).ToList();
        for (int i = 0; i < clses.Count; i++)
        {
            if (i == 0)
            {
                prevCls = clses[i];
                continue;
            }
            BeatCubeClass cls = editor.project.beatCubeList[i];
            if(cls.road == prevCls.road && cls.level == prevCls.level && Mathf.Abs(cls.time - prevCls.time) <= 0.08f)
            {
                items.Add(new WarningItem(@"Кубы слишком близко
(" + TheGreat.SecondsToTime(cls.time) + " на " + cls.road + " дорожке)", cls));
            }
            prevCls = cls;
        }
        RefreshItems();
    }
    void RefreshItems()
    {
        GameObject prefab = transform.GetChild(0).gameObject;
        prefab.SetActive(true);

        foreach (Transform child in transform) if (child.name != prefab.name) Destroy(child.gameObject);

        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = Instantiate(prefab, transform);
            item.name = "Item" + i;
            item.transform.GetChild(2).GetComponent<Text>().text = items[i].msg;
        }

        prefab.SetActive(false);
    }
    public void OnItemClicked(Transform self)
    {
        editor.aSource.time = items[int.Parse(self.name.Replace("Item", ""))].go.time;
    }
}

public class WarningItem
{
    public string msg;
    public BeatCubeClass go;

    public WarningItem(string msg, BeatCubeClass go)
    {
        this.msg = msg;
        this.go = go;
    }
}