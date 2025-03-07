
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.HeadTags
{
    public class TagsManager : UdonSharpBehaviour
    {
        public TagsInfo tagsInfoPrefab;
        public Tag tagPrefab;
        public TagsInfo[] tagsInfos;
        public string[] haveTagsPlayerNames;
        [NonSerialized] DataDictionary tagsData;
        public TextAsset localData;
        void Start()
        {
            if (localData != null
            && VRCJson.TryDeserializeFromJson(localData.text, out var tagsDataToken))
            {
                tagsData = tagsDataToken.DataDictionary;
                UpdateData();
            }
        }
        void Update()
        {
            if (tagsInfos == null || tagsInfos.Length == 0) { return; }
            var localPlayerHead = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
            foreach (var tagsInfo in tagsInfos)
            {
                if (tagsInfo == null) { continue; }
                tagsInfo.UpdatePosition(localPlayerHead);
            }
        }
        public void UpdateData() => UpdateData(tagsData);
        public void UpdateData(DataDictionary data)
        {
            var _players = GetPlayers();
            // 不在场景里的玩家，新增的玩家
            var haveTagsPlayersInt = new DataList();
            // var leavePlayersInt = new DataList();
            var keys = data.GetKeys();
            // if (haveTagsPlayerNames == null || haveTagsPlayerNames.Length == 0)
            // {
            for (var i = 0; i < _players.Length; i++)
            {
                var player = _players[i];
                if (player == null) { continue; }
                if (!keys.Contains(player.displayName)) { continue; }
                haveTagsPlayersInt.Add(i);
            }
            haveTagsPlayerNames = new string[haveTagsPlayersInt.Count];
            ClearTagsInfos();
            tagsInfos = new TagsInfo[haveTagsPlayersInt.Count];
            for (var i = 0; i < haveTagsPlayersInt.Count; i++)
            {
                var haveTagsPlayerName = _players[haveTagsPlayersInt[i].Int].displayName;
                haveTagsPlayerNames[i] = haveTagsPlayerName;
                var tagsInfo = (TagsInfo)Instantiate(tagsInfoPrefab.gameObject, transform).GetComponent(typeof(UdonBehaviour));
                tagsInfo.gameObject.SetActive(true);
                tagsInfo.tagsManager = this;
                tagsInfo.SetPlayer(_players[haveTagsPlayersInt[i].Int]);
                if (data.TryGetValue(haveTagsPlayerName, out var tagsToken))
                {
                    tagsInfo.SetData(tagsToken.DataDictionary);
                }
                tagsInfos[i] = tagsInfo;
            }
            // }
            // else
            // {
            //     for (var i = 0; i < _players.Length; i++)
            //     {
            //         var player = _players[i];
            //         if (player == null
            //         || !keys.Contains(player.displayName)
            //         || Array.IndexOf((Array)haveTagsPlayerNames, player.displayName) != -1) { continue; }
            //         haveTagsPlayersInt.Add(i);
            //     }
            //     var _playerNames = GetPlayerNames(_players);
            //     for (var i = 0; i < haveTagsPlayerNames.Length; i++)
            //     {
            //         var playerName = haveTagsPlayerNames[i];
            //         if (string.IsNullOrEmpty(playerName)
            //         || !keys.Contains(playerName)
            //         || Array.IndexOf((Array)_playerNames, playerName) == -1)
            //         {
            //             leavePlayersInt.Add(i);
            //             continue;
            //         }
            //     }
            // }

        }
        void ClearTagsInfos()
        {
            if (tagsInfos == null || tagsInfos.Length == 0) { return; }
            foreach (var tagsInfo in tagsInfos)
            {
                Destroy(tagsInfo.gameObject);
            }
            tagsInfos = new TagsInfo[0];
        }
        VRCPlayerApi[] GetPlayers()
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            players = VRCPlayerApi.GetPlayers(players);
            return players;
        }
        // string[] GetPlayerNames(VRCPlayerApi[] players)
        // {
        //     var names = new string[players.Length];
        //     for (var i = 0; i < players.Length; i++)
        //     {
        //         names[i] = players[i].displayName;
        //     }
        //     return names;
        // }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            UpdateData();
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player.isLocal) { return; }
            base.OnPlayerLeft(player);
            UpdateData();
        }
    }
}
