using Assets.AccountManagement;
using CoversManagement;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using ProjectManagement;
using GameNet;
using GameNet.Operations;
using System.Threading.Tasks;


public class PublishedProjectUI : MonoBehaviour
{
    public AccountManager manager;

    public Transform projectListContent;
    public GameObject noProjectsYetText, youMustAuthText;
   

    public async void ShowProjects()
    {
        foreach (Transform child in projectListContent) if (child.name != "ProjectItem") Destroy(child.gameObject);
        GameObject prefab = projectListContent.GetChild(0).gameObject;
        prefab.SetActive(false);

        Camera.main.GetComponent<ModerationUI>().GetOperationStates();

        if (AccountManager.LoggedAccount == null)
        {
            noProjectsYetText.SetActive(false);
            youMustAuthText.SetActive(true);
            return;
        }
        youMustAuthText.SetActive(false);

        prefab.SetActive(true);


        MapInfo[] projects = await GetPublishedMaps();

        projectListContent.GetComponent<RectTransform>().sizeDelta = new Vector2(projectListContent.GetComponent<RectTransform>().sizeDelta.x, -8);
        List<CoverRequestPackage> coverRequestPackages = new List<CoverRequestPackage>();
        foreach (var project in projects)
        {
            GameObject go = Instantiate(prefab, projectListContent);
            PublishedProjectItem item = go.GetComponent<PublishedProjectItem>();
            item.mapInfo = project;
            coverRequestPackages.Add(new CoverRequestPackage(item.coverImage, item.mapInfo.group.author + "-" + item.mapInfo.group.name, item.mapInfo.nick));
            item.Refresh();

            projectListContent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 375 + 8);
        }

        CoversManager.AddPackages(coverRequestPackages);

        noProjectsYetText.SetActive(projects.Length == 0);

        prefab.SetActive(false);
    }


    public async Task<MapInfo[]> GetPublishedMaps()
    {
        OperationMessage msg = await NetCore.ServerActions.Account.GetPublishedMaps(AccountManager.LoggedAccount.Nick, "");

        if (msg.Type != OperationType.Success) return null;



        MapInfo[] arr = JsonConvert.DeserializeObject<MapInfo[]>(msg.Message);

        foreach (var item in arr)
        {
            item.publishTime = item.publishTime.ToLocalTime();
        }

        return arr;
    }
}
