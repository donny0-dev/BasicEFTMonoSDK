using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using Comfort.Common;
using EFT.NextObservedPlayer;
using BSG.CameraEffects;
using System.Reflection;
using EFT.Interactive;

namespace BasicMonoSDK
{
    public static class Globals
    {
        public static Camera MainCamera;
        public static GameWorld GameWorld;
        public static Player LocalPlayer;

        public static List<IPlayer> Players = new List<IPlayer>();
        public static List<Throwable> Grenades = new List<Throwable>();

        public static bool IsMenuOpen = false;

        public static Vector3 W2S(Vector3 pos)
        {
            if (!Globals.MainCamera)
                return new Vector3(0, 0, 0);

            var WS2P = Globals.MainCamera.WorldToScreenPoint(pos);
            WS2P.y = Screen.height - WS2P.y;

            if (WS2P.z < 0.001f)
                return new Vector3(0, 0, 0);

            return WS2P;
        }

        // Not the best way but works.
        public static bool IsBossByName(string name)
        {
            if (name == "Килла" || name == "Решала" || name == "Глухарь" || name == "Штурман" || name == "Санитар" || name == "Тагилла" || name == "Зрячий" || name == "Кабан" || name == "Big Pipe" || name == "Birdeye" || name == "Knight" || name == "Дед Мороз" || name == "Коллонтай")
                return true;
            else
                return false;
        }

        internal static void SetPrivateField(this object obj, string name, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            fieldInfo.SetValue(obj, value);
        }
    }

    public static class MenuVars
    {
        public static bool EnableESP = true;
        public static bool EnableGrenadeESP = true;

        public static float MaxScavRenderDistance = 200f;

        public static bool ForceNightVision = false;
        public static bool ForceThermalVision = false;
    }

    public class Cheat : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.Label(new Rect(10f, 10f, 300f, 100f), "BasicMonoSDK");

            if (Globals.IsMenuOpen)
            {
                GUI.Box(new Rect(100f, 50f, 400f, 400f), " ");
                GUILayout.BeginArea(new Rect(100f, 50f, 400f, 400f));

                MenuVars.EnableESP = GUILayout.Toggle(MenuVars.EnableESP, "Player ESP");
                MenuVars.EnableGrenadeESP = GUILayout.Toggle(MenuVars.EnableGrenadeESP, "Grenade ESP");

                GUILayout.Label($"Scav Render Distance {Math.Round(MenuVars.MaxScavRenderDistance)}");
                MenuVars.MaxScavRenderDistance = GUILayout.HorizontalScrollbar(MenuVars.MaxScavRenderDistance, 1f, 1f, 1500f, GUILayout.MaxWidth(250f));

                MenuVars.ForceNightVision = GUILayout.Toggle(MenuVars.ForceNightVision, "Force Night Vision");
                MenuVars.ForceThermalVision = GUILayout.Toggle(MenuVars.ForceThermalVision, "Force Thermal Vision");

                GUILayout.EndArea();
            }

            if (MenuVars.EnableESP)
                PlayerESP();

            if (MenuVars.EnableGrenadeESP)
                GrenadeESP();
        }

        private void Update()
        {
            float LastCacheTime = 0f;

            if (Input.GetKeyUp(KeyCode.Insert))
                Globals.IsMenuOpen = !Globals.IsMenuOpen;

            if (Globals.IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Updates every 0.25f seconds.
            if (Time.time >= LastCacheTime)
            {
                if (Camera.main != null)
                    Globals.MainCamera = Camera.main;

                if (Singleton<GameWorld>.Instance != null)
                    Globals.GameWorld = Singleton<GameWorld>.Instance;

                if (Globals.GameWorld != null && Globals.GameWorld.RegisteredPlayers != null)
                {
                    List<IPlayer> RegisteredPlayers = Globals.GameWorld.RegisteredPlayers;

                    Globals.Players.Clear();
                    Globals.Grenades.Clear();

                    foreach (var Player in RegisteredPlayers)
                    {
                        if (Player == null)
                            continue;

                        if (Player.IsYourPlayer)
                            Globals.LocalPlayer = Player as Player;

                        Globals.Players.Add(Player);
                    }

                    for (int i = 0; i < Globals.GameWorld.Grenades.Count; i++)
                    {
                        Throwable Throwables = Globals.GameWorld.Grenades.GetByIndex(i);
                        if (Throwables == null)
                            continue;

                        Globals.Grenades.Add(Throwables);
                    }
                }
                else
                {
                    Globals.Players.Clear();
                    Globals.Grenades.Clear();
                }

                LastCacheTime = Time.time + 0.25f;
            }

            if (Globals.MainCamera != null)
            {
                Globals.MainCamera.GetComponent<NightVision>().SetPrivateField("_on", MenuVars.ForceNightVision);

                Globals.MainCamera.GetComponent<ThermalVision>().On = MenuVars.ForceThermalVision;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void GrenadeESP()
        {
            if (Globals.GameWorld == null || Globals.LocalPlayer == null || Globals.MainCamera == null || Globals.Grenades.IsNullOrEmpty())
                return;

            for (int i = 0; i < Globals.Grenades.Count(); i++)
            {
                Throwable Throwables = Globals.Grenades.ElementAt(i);

                if (Throwables == null)
                    continue;

                Grenade Grenades = Throwables as Grenade;

                if (Grenades == null)
                    continue;

                Vector3 GrenadePos = Globals.W2S(Grenades.transform.position);

                if (GrenadePos == Vector3.zero)
                    continue;

                GUI.Label(new Rect(GrenadePos.x, GrenadePos.y, 200f, 25f), Grenades.name);
            }
        }

        private void PlayerESP()
        {
            if (Globals.GameWorld == null || Globals.LocalPlayer == null || Globals.MainCamera == null || Globals.Players.IsNullOrEmpty())
                return;

            for (int i = 0; i < Globals.Players.Count(); i++)
            {
                IPlayer _Player = Globals.Players.ElementAt(i);

                if (_Player == null)
                    continue;

                // FOR ONLINE RAIDS
                if (_Player.GetType() != typeof(ObservedPlayerView))
                    continue;

                ObservedPlayerView Player = _Player as ObservedPlayerView;
                if (Player == null)
                    continue;
                // FOR ONLINE RAIDS

                // FOR OFFLINE RAIDS
                //Player OfflinePlayer = _Player as Player;
                //if (Player == null)
                //    continue;
                // FOR OFFLINE RAIDS

                Vector3 HeadPos = Globals.W2S(Player.PlayerBones.Head.position);

                if (HeadPos == Vector3.zero)
                    continue;

                float Distance2Player = Vector3.Distance(Globals.MainCamera.transform.position, Player.PlayerBones.Head.position);

                bool IsScav = false;
                if (Player.ObservedPlayerController != null && Player.ObservedPlayerController.InfoContainer != null)
                    IsScav = Player.ObservedPlayerController.InfoContainer.Side == EPlayerSide.Savage;

                // Not the best way, see if you (the reader) can improve this.
                bool IsBoss = Globals.IsBossByName(Player.NickName.Localized());

                if (IsBoss)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(HeadPos.x, HeadPos.y, 200f, 25f), "BOSS " + Math.Round(Distance2Player).ToString() + "M");
                    GUI.color = Color.white;
                }
                else if (IsScav)
                {
                    if (Distance2Player <= MenuVars.MaxScavRenderDistance)
                    {
                        GUI.color = Color.magenta;
                        GUI.Label(new Rect(HeadPos.x, HeadPos.y, 200f, 25f), "SCAV " + Math.Round(Distance2Player).ToString() + "M");
                        GUI.color = Color.white;
                    }
                }
                else
                {
                    GUI.Label(new Rect(HeadPos.x, HeadPos.y, 200f, 25f), Player.NickName + " " + Math.Round(Distance2Player).ToString() + "M");
                }
            }
        }
    }
}
