using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Networking;

namespace QuickStart
{
    public class DodgeballScript : NetworkBehaviour
    {
        [SyncVar] private GameObject objectId;
        private NetworkIdentity objNetID;
        private float ballSpeed = 4f;
        private int[] direction = { -1, 1 };
        [SyncVar] public int ballDirection = 0;
        [SyncVar] public float ballRotation = 0;

        private SceneScript sceneScript;

        private bool sentPlayerData = false;

        private void Awake()
        {
            sceneScript = GameObject.Find("SceneReference").GetComponent<SceneReference>().sceneScript;
        }

        private void Start()
        {
            objectId = this.gameObject;
            ballDirection = 1;
            //Random.Range(0, 2) == 0 ? -1 : 1;
            this.gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 255);
        }

        private void Update()
        {
            objectId.transform.position += new Vector3(ballRotation * Time.deltaTime, 0, ballDirection * Time.deltaTime * ballSpeed);

            if (this.gameObject.transform.position.x >= 10 || this.gameObject.transform.position.x <= -10)
            {
                rpcFlipBallDirection();
            }

            if(this.gameObject.transform.position.z >= 10 || this.gameObject.transform.position.z <= -10)
            {
                //Send player name who won to database.

                if(sentPlayerData == false)
                {
                    string[] statusText = sceneScript.canvasStatusText.text.Split(' ');
                    var playerName = statusText[0];

                    sceneScript.canvasStatusText.text = playerName.ToString() + " has won.";

                    DataClass playerData = new DataClass();
                    playerData.playerName = playerName;
                    playerData.playerWins = 1;
                    playerData.gamesPlayed = 1;

                    sentPlayerData = true;
                    StartCoroutine(SendWebData(JsonUtility.ToJson(playerData)));
                    Time.timeScale = 0.1f;
                }
            }
        }

        [ClientRpc]
        public void rpcFlipBallDirection()
        {
            ballRotation = -ballRotation;
        }

        [ClientRpc]
        public void rpcChangeBallDirection()
        {
            ballDirection = -ballDirection;
            ballRotation = Random.Range(-3f, 4f);
            ballSpeed += .5f;
        }

        IEnumerator SendWebData(string json)
        {
            using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:3000/unitySave", json))
            {
                request.SetRequestHeader("content-type", "application/json");
                request.uploadHandler.contentType = "application/json";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    Debug.Log("DataObj Posted");
                }
                request.uploadHandler.Dispose();
            }
        }
    }
}
