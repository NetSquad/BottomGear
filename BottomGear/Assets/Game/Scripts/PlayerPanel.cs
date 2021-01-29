// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerNumbering.cs" company="Exit Games GmbH">
//   Part of: Asteroid Demo,
// </copyright>
// <summary>
//  Player Overview Panel
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;

namespace BottomGear
{
    public class PlayerPanel : MonoBehaviourPunCallbacks
    {
        public GameObject PlayerOverviewEntryPrefab;

        public Dictionary<int, GameObject> playerListEntries;

        #region UNITY

        public void Awake()
        {
            playerListEntries = new Dictionary<int, GameObject>();

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerOverviewEntryPrefab);
                entry.transform.SetParent(gameObject.transform);
                //entry.transform.localScale = Vector3.one;
                //entry.GetComponent<Text>().color = AsteroidsGame.GetColor(p.GetPlayerNumber());
                entry.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0}\nScore: {1}\n", p.NickName, p.GetScore());
                playerListEntries.Add(p.ActorNumber, entry);
            }
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {

            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                entry.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("{0}\nScore: {1}\n", targetPlayer.NickName, targetPlayer.GetScore());
            }
        }

        #endregion
    }
}