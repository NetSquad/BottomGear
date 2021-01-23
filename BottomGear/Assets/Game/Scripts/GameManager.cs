// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsteroidsGameManager.cs" company="Exit Games GmbH">
//   Part of: Asteroid demo
// </copyright>
// <summary>
//  Game Manager for the Asteroid Demo
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

namespace BottomGear
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance = null;
        public Transform MainSpawn;

        public GameObject sceneCamera;

        public Text InfoText;
        public Text Timer;

        public double timeLimit = 10.0f;
        double initialTime = 0.0f;
        double currentTime = 0.0f;
        bool startTimer = false;

        //public GameObject[] AsteroidPrefabs; // unneeded

        #region UNITY

        public void Awake()
        {
            Instance = this;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
        }

        public void Start()
        {
            // NOTE: Without this the player's spaceship won't spawn, check OnPlayerPropertiesUpdate below
            // it will look for PLAYER_LOADED_LEVEL flag

            Hashtable props = new Hashtable
            {
                {Photon.Pun.Demo.Asteroids.AsteroidsGame.PLAYER_LOADED_LEVEL, true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);


        }

        public override void OnDisable()
        {
            base.OnDisable();

            CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
        }

        #endregion

        #region COROUTINES

        //private IEnumerator SpawnAsteroid()
        //{
        //    while (true)
        //    {
        //        yield return new WaitForSeconds(Random.Range(AsteroidsGame.ASTEROIDS_MIN_SPAWN_TIME, AsteroidsGame.ASTEROIDS_MAX_SPAWN_TIME));

        //        Vector2 direction = Random.insideUnitCircle;
        //        Vector3 position = Vector3.zero;

        //        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        //        {
        //            // Make it appear on the left/right side
        //            position = new Vector3(Mathf.Sign(direction.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, direction.y * Camera.main.orthographicSize);
        //        }
        //        else
        //        {
        //            // Make it appear on the top/bottom
        //            position = new Vector3(direction.x * Camera.main.orthographicSize * Camera.main.aspect, 0, Mathf.Sign(direction.y) * Camera.main.orthographicSize);
        //        }

        //        // Offset slightly so we are not out of screen at creation time (as it would destroy the asteroid right away)
        //        position -= position.normalized * 0.1f;


        //        Vector3 force = -position.normalized * 1000.0f;
        //        Vector3 torque = Random.insideUnitSphere * Random.Range(500.0f, 1500.0f);
        //        object[] instantiationData = { force, torque, true };

        //        PhotonNetwork.InstantiateRoomObject("BigAsteroid", position, Quaternion.Euler(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0, instantiationData);
        //    }
        //}

        private IEnumerator EndOfGame(string winner, int score)
        {
            float timer = 5.0f;

            while (timer > 0.0f)
            {
                InfoText.text = string.Format("Player {0} won with {1} points.\n\n\nReturning to login screen in {2} seconds.", winner, score, timer.ToString("n2"));

                yield return new WaitForEndOfFrame();

                timer -= Time.deltaTime;
            }

            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnDisconnected(DisconnectCause cause)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            //if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            //{
            //    StartCoroutine(SpawnAsteroid());
            //}
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            CheckEndOfGame();
        }

        private void Update()
        {

            if (!startTimer || currentTime >= timeLimit)
            {
                return;
            }

            currentTime = PhotonNetwork.Time - initialTime;

            Timer.text = string.Format("{0}", System.Math.Round(currentTime, 2));

            if (currentTime >= timeLimit)
            {
                currentTime = timeLimit;

                if (PhotonNetwork.IsMasterClient)
                {
                    StopAllCoroutines();
                }

                string winner = "";
                int score = -1;

                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (p.GetScore() > score)
                    {
                        winner = p.NickName;
                        score = p.GetScore();
                    }
                }

                StartCoroutine(EndOfGame(winner, score));
            }

        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log("CountdownTimer.OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());

            if (propertiesThatChanged.ContainsKey("GameStart"))
            {
                object propsTime;
                
                if (propertiesThatChanged.TryGetValue("GameStart", out propsTime))
                {
                    initialTime = (double)propsTime;
                    startTimer = true;
                }

                Debug.Log("Retrieved time from room properties");
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // --- NOTE: This is AsteroidsGame's game code ---

            if (changedProps.ContainsKey("score") || changedProps.ContainsKey("GameStart"))
            {
                
                return;
            }

            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                return;
            }


            // --- NOTE: This is AsteroidsGame's game code ---
            // --- If flag PLAYER_LOADED_LEVEL is true, begin counter to game start ---


            // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
            int startTimestamp;
            bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);


            if (changedProps.ContainsKey(Photon.Pun.Demo.Asteroids.AsteroidsGame.PLAYER_LOADED_LEVEL))
            {
                if (CheckAllPlayerLoadedLevel())
                {
                    if (!startTimeIsSet)
                    {
                        CountdownTimer.SetStartTime();
                    }
                }
                else
                {
                    // not all players loaded yet. wait:
                    Debug.Log("setting text waiting for players! ", this.InfoText);
                    InfoText.text = "Waiting for other players...";
                }
            }

        }

        #endregion


        // called by OnCountdownTimerIsExpired() when the timer ended
        private void StartGame()
        {
            Debug.Log("StartGame!");

            // --- NOTE: Called when starting countdown finishes ---

            sceneCamera.SetActive(false);

            // on rejoin, we have to figure out if the spaceship exists or not
            // if this is a rejoin (the ship is already network instantiated and will be setup via event) we don't need to call PN.Instantiate


            // --- NOTE: This is AsteroidsGame's game code, spawn ship and start asteroid spwaning coroutine ---


            float angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
            float x = 20.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
            float z = 20.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
            Vector3 position = new Vector3(x, MainSpawn.position.y, z);
            Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);

            PhotonNetwork.Instantiate("BattleRoller", position, rotation, 0);      // avoid this call on rejoin (ship was network instantiated before)
            //BasicKyle Variant
            //if (PhotonNetwork.IsMasterClient)
            //{
            //    StartCoroutine(SpawnAsteroid());
            //}

            // --- Start game timer ---
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Hashtable ht = new Hashtable();
                initialTime = PhotonNetwork.Time;
                startTimer = true;
                ht.Add("GameStart", initialTime);
                PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
            }
            //else
            //{
            //    initialTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
            //    startTimer = true;
            //}

        }

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {

                // --- NOTE: This is AsteroidsGame's game code ---

                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(Photon.Pun.Demo.Asteroids.AsteroidsGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool)playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }

        private void CheckEndOfGame()
        {
            //bool allDestroyed = true;

            foreach (Player p in PhotonNetwork.PlayerList)
            {

                // --- NOTE: This is AsteroidsGame's game code ---
                //object lives;

                //if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_LIVES, out lives))
                //{
                //    if ((int)lives > 0)
                //    {
                //        allDestroyed = false;
                //        break;
                //    }
                //}
            }

            if (currentTime >= timeLimit || PhotonNetwork.PlayerList.Length <= 1/*allDestroyed*/)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StopAllCoroutines();
                }

                string winner = "";
                int score = -1;

                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (p.GetScore() > score)
                    {
                        winner = p.NickName;
                        score = p.GetScore();
                    }
                }

                StartCoroutine(EndOfGame(winner, score));
            }
        }

        private void OnCountdownTimerIsExpired()
        {
            StartGame();
        }
    }
}