using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMesh playerNameText;
        public GameObject floatingInfo;

        private Material playerMaterialClone;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        [SerializeField]
        private SceneScript sceneScript;

        //Holds amount of players in server
        private static int count = 0;

        //Holds where the center of the map is, ball will spawn here and will players will look here originally.
        [SerializeField]
        private GameObject centerMap;

        [SyncVar]
        public string lastHitBall = string.Empty;

        //Jump Vars

        void Awake()
        {
            //allow all players to run this
            sceneScript = GameObject.Find("SceneReference").GetComponent<SceneReference>().sceneScript;

            count++;
        }

        private void Start()
        {
            if (isLocalPlayer)
            {
                transform.name = "NetworkPlayer (Local)";
            }
            else
                transform.name = "NetworkPlayer (Network)";

            print($"{transform.name}, total players: {count}");
        }

        private void OnDestroy()
        {
            count--;
        }

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }


        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            playerMaterialClone = new Material(GetComponent<Renderer>().material);
            playerMaterialClone.color = _New;
            GetComponent<Renderer>().material = playerMaterialClone;
        }

        

        public override void OnStartLocalPlayer()
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);

            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            sceneScript.playerScript = this;

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);

            this.gameObject.transform.LookAt(centerMap.transform);
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            // player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            sceneScript.statusText = $"{playerName} joined.";
        }

        [Command]
        public void CmdSendPlayerMessage()
        {
            if (sceneScript)
                sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                // make non-local players run this
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }

            //Moves player
            float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

            #region Player Boundaries
            if (this.gameObject.transform.position.x <= 10)
            {
                transform.Translate(moveZ, 0, 0);
            }
            else
            {
                this.gameObject.transform.position = new Vector3(10f, this.gameObject.transform.position.y,
                    this.gameObject.transform.position.z);
            }
            if(this.gameObject.transform.position.x >= -10f)
            {
                transform.Translate(moveZ, 0, 0);
            }
            else
            {
                this.gameObject.transform.position = new Vector3(-10f, this.gameObject.transform.position.y,
                    this.gameObject.transform.position.z);
            }
            #endregion

        }

        [Command]
        private void cmdSetHitLast(string player)
        {
            lastHitBall = player.ToString();
            sceneScript.canvasStatusText.text = player + " hit the ball.";
            rpcSetHitLast(player);
        }

        [ClientRpc]
        private void rpcSetHitLast(string player)
        {
            sceneScript.canvasStatusText.text = player + " hit the ball.";
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent<DodgeballScript>(out var ballScript))
            {
                ballScript.rpcChangeBallDirection();
            }
            else
                return;

            cmdSetHitLast(this.playerName);
        }
    }
}
