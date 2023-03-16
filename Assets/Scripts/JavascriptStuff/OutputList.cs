using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;
using TMPro;

public class OutputList : MonoBehaviour
{
    public GameObject displayList;

    public void Start()
    {
        StartCoroutine(GetRequest("http://localhost:3000/getPlayersTop10"));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest getRequest = UnityWebRequest.Get(uri))
        {
            yield return getRequest.SendWebRequest();

            var newData = System.Text.Encoding.UTF8.GetString(getRequest.downloadHandler.data);
            var newGetRequestData = JsonUtility.FromJson<ListUser>(newData);

            Debug.Log(newData); 

            if (getRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(getRequest.error);
            }

            int n = newGetRequestData.game.Length;
            int index = 0;

            foreach (DataClass d in newGetRequestData.game)
            {
                GameObject display = displayList;

                display.GetComponentInChildren<TextMeshProUGUI>().text = index + 1 + ". " + d.playerName + " - " + d.playerWins
                    + " - " + d.gamesPlayed;

                //display.GetComponentInChildren<Deleting>().id.holder = d._id;

                Instantiate(display, Vector3.zero, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);

                index++;
                if (n >= 10)
                {
                    if(index == 10)
                    {
                        break;
                    }
                }
            }
        }
    }
}
