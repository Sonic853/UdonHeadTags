
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.HeadTags
{
    public class TagsInfo : UdonSharpBehaviour
    {
        public TagsManager tagsManager;
        public Transform target;
        /// <summary>
        /// 离头部的Y轴距离
        /// </summary>
        public float headDistance;
        Tag tagPrefab => tagsManager.tagPrefab;
        public RectTransform tagsContent;
        public Tag[] tags;
        // public string[] tags;
        // public Color[] colors;
        // public Color[] bgcolors;
        public VRCPlayerApi player;
        string i18n = "";
        public void UpdatePosition(Vector3 localPlayerHead)
        {
            if (player == null) { return; }
            if (target == null) target = transform;
            var headPositon = player.GetBonePosition(HumanBodyBones.Head);
            target.position = headPositon + Vector3.up * headDistance;
            var oldRotation = target.rotation;
            if (player != Networking.LocalPlayer)
            {
                target.LookAt(localPlayerHead);
                // 只保留X和Z轴旋转
                target.rotation = Quaternion.Euler(oldRotation.eulerAngles.x, target.rotation.eulerAngles.y, oldRotation.eulerAngles.z);
            }
            else
            {
                // 获得本地玩家的面朝向
                var localPlayerRotation = Networking.LocalPlayer.GetBoneRotation(HumanBodyBones.Head);
                // 只保留X和Z轴旋转
                target.rotation = Quaternion.Euler(oldRotation.eulerAngles.x, localPlayerRotation.eulerAngles.y, oldRotation.eulerAngles.z);
            }
        }
        public void SetPlayer(VRCPlayerApi _player) => player = _player;
        public void SetData(DataDictionary data)
        {
            if (string.IsNullOrEmpty(i18n)) i18n = VRCPlayerApi.GetCurrentLanguage();
            if (data.TryGetValue("d", out var distanceToken))
            {
                Debug.Log(distanceToken.TokenType);
                headDistance = (float)distanceToken.Double;
            }
            if (data.TryGetValue("t", out var tagsToken))
            {
                var _tags = tagsToken.DataList;
                ClearTags();
                tags = new Tag[_tags.Count];
                // colors = new Color[_tags.Count];
                for (int i = 0; i < _tags.Count; i++)
                {
                    var item = _tags[i].DataDictionary;
                    if (!item.TryGetValue(i18n, out var tagToken))
                        item.TryGetValue("n", out tagToken);
                    var tag = (Tag)Instantiate(tagPrefab.gameObject, tagsContent).GetComponent(typeof(UdonBehaviour));
                    tag.textUI.text = tagToken.String;
                    tag.textUI.color = Color.white;
                    if (item.TryGetValue("b", out var bgcolorToken))
                        tag.bgUI.color = HexToColor(bgcolorToken.String);
                    if (item.TryGetValue("c", out var colorToken))
                        tag.textUI.color = HexToColor(colorToken.String);
                    tags[i] = tag;
                }
            }
        }
        void ClearTags()
        {
            if (tags == null || tags.Length == 0) { return; }
            foreach (var tag in tags)
            {
                Destroy(tag.gameObject);
            }
            tags = new Tag[0];
        }
        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Color.white;
            hex = hex.ToUpper();
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 3) // #RGB -> #RRGGBB
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}FF"; // 添加默认不透明
            }
            else if (hex.Length == 4) // #RGBA -> #RRGGBBAA
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
            }
            else if (hex.Length == 6) // #RRGGBB -> #RRGGBBAA
            {
                hex += "FF"; // 默认不透明
            }
            else if (hex.Length != 8)
            {
                return Color.white;
            }

            var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            var a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, a);
        }
    }
}
