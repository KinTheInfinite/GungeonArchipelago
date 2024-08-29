using BepInEx;
using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Alexandria.ItemAPI;
using static DirectionalAnimation;
using System.Net.Sockets;

namespace GungeonArchipelago
{
    [BepInDependency(Alexandria.Alexandria.GUID)]
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "kintheinfinite.etg.archipelago";
        public const string NAME = "Archipelago";
        public const string VERSION = "1.0.0";
        public const string TEXT_COLOR = "#FFFFFF";
        public static ArchipelagoSession session;
        private static int chest_starting_id = 8755000;
        public static Dictionary<string, int> slot_data = new Dictionary<string, int>();
        public static List<long> items_received = new List<long>();
        public static System.Random random = new System.Random();
        public static PassiveItem archipelago_pickup;
        public static bool allow_traps = false;

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            ETGModConsole.CommandDescriptions.Add("archipelago connect", "Input the ip, port, player name, and password seperated by spaces");
            ETGModConsole.CommandDescriptions.Add("archipelago items", "Use when you are starting a new run but are already connected to spawn your items in");
            ETGModConsole.CommandDescriptions.Add("archipelago chat", "Sends a chat message to your connected server");
            ETGModConsole.Commands.AddGroup("archipelago");
            ETGModConsole.Commands.GetGroup("archipelago").AddGroup("connect", (string[] args) => ArchipelagoConnect(args));
            ETGModConsole.Commands.GetGroup("archipelago").AddGroup("items", (string[] args) => ArchipelagoItems());
            ETGModConsole.Commands.GetGroup("archipelago").AddGroup("chat", (string[] args) => ArchipelagoChat(args[0]));
            ETGMod.Chest.OnPostOpen += OnChestOpen;
            // For sprite purposes on items received and sent (should probably be its own class)
            string item_name = "Archipelago Item";
            string resource_name = "GungeonArchipelago/Resources/archipelago_sprite";
            GameObject obj = new GameObject(item_name);
            archipelago_pickup = obj.AddComponent<PassiveItem>();
            ItemBuilder.AddSpriteToObject(item_name, resource_name, obj);
            ItemBuilder.SetupItem(archipelago_pickup, "An archipelago item.", "An archipelago item\n\nA randomized item received from the archipelago server", "Archipelago");
            // Don't allow this to be acquired as an actual item
            archipelago_pickup.quality = PickupObject.ItemQuality.EXCLUDED;
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }

        public void Update()
        {
            PlayerController player = GameManager.Instance.m_player;
            if (player != null)
            {
                player.OnKilledEnemyContext -= OnEnemyKill;
                player.OnKilledEnemyContext += OnEnemyKill;
            }
            if (items_received.Count == 0)
            {
                return;
            }
            /*for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
            {
                PickupObject obj = PickupObjectDatabase.Instance.Objects[i];
                if (obj == null || obj.DisplayName == null || obj.tag == null)
                {
                    continue;
                }
                Log(obj.DisplayName + " " + obj.quality);
            }*/
            long received_item = items_received[0];
            items_received.RemoveAt(0);
            // Definitely a better way to do this, also shouldn't be using the console really
            switch (received_item)
            {
                // Random D Tier Gun
                case 8754000:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomGunOfQualities(random, new List<int>(), PickupObject.ItemQuality.D).gameObject, 
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random C Tier Gun
                case 8754001:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomGunOfQualities(random, new List<int>(), PickupObject.ItemQuality.C).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random B Tier Gun
                case 8754002:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomGunOfQualities(random, new List<int>(), PickupObject.ItemQuality.B).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random A Tier Gun
                case 8754003:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomGunOfQualities(random, new List<int>(), PickupObject.ItemQuality.A).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random S Tier Gun
                case 8754004:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomGunOfQualities(random, new List<int>(), PickupObject.ItemQuality.S).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random D Tier Item
                case 8754005:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomPassiveOfQualities(random, new List<int>(), PickupObject.ItemQuality.D).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random C Tier Item
                case 8754006:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomPassiveOfQualities(random, new List<int>(), PickupObject.ItemQuality.C).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random B Tier Item
                case 8754007:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomPassiveOfQualities(random, new List<int>(), PickupObject.ItemQuality.B).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random A Tier Item
                case 8754008:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomPassiveOfQualities(random, new List<int>(), PickupObject.ItemQuality.A).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Random S Tier Item
                case 8754009:
                    LootEngine.SpawnItem(PickupObjectDatabase.GetRandomPassiveOfQualities(random, new List<int>(), PickupObject.ItemQuality.S).gameObject,
                        GameManager.Instance.PrimaryPlayer.CenterPosition, Vector2.down, 0f, true, false, false);
                    break;
                // Gnawed Key
                case 8754010:
                    ETGModConsole.SpawnItem(new string[] { "gnawed_key", "1" });
                    break;
                // Old Crest
                case 8754011:
                    ETGModConsole.SpawnItem(new string[] { "old_crest", "1" });
                    break;
                // Weird Egg
                case 8754012:
                    ETGModConsole.SpawnItem(new string[] { "weird_egg", "1" });
                    break;
                // Chancekin Party
                case 8754100:
                    ETGModConsole.Spawn(new string[] { "chance_kin", "10" });
                    break;
                // 50 Casings
                case 8754101:
                    ETGModConsole.SpawnItem(new string[] { "50_casing", "1" });
                    break;
                // Key
                case 8754102:
                    ETGModConsole.SpawnItem(new string[] { "key", "1" });
                    break;
                // Blank
                case 8754103:
                    ETGModConsole.SpawnItem(new string[] { "blank", "1" });
                    break;
                // Armor
                case 8754104:
                    ETGModConsole.SpawnItem(new string[] { "armor", "1" });
                    break;
                // Heart
                case 8754105:
                    ETGModConsole.SpawnItem(new string[] { "heart", "1" });
                    break;
                // Ammo
                case 8754106:
                    ETGModConsole.SpawnItem(new string[] { "ammo", "1" });
                    break;
                // Rat Invasion
                case 8754200:
                    ETGModConsole.Spawn(new string[] { "rat", "100" });
                    break;
                // Shelleton Men
                case 8754201:
                    ETGModConsole.Spawn(new string[] { "shelleton", "3" });
                    break;
                // Shotgrub Storm
                case 8754202:
                    ETGModConsole.Spawn(new string[] { "shotgrub", "3" });
                    break;
                // Tanker Battalion
                case 8754203:
                    ETGModConsole.Spawn(new string[] { "tanker", "12" });
                    ETGModConsole.Spawn(new string[] { "professional", "2" });
                    break;
                // Ghost Party
                case 8754204:
                    ETGModConsole.Spawn(new string[] { "hollowpoint", "6" });
                    ETGModConsole.Spawn(new string[] { "bombshee", "2" });
                    ETGModConsole.Spawn(new string[] { "gunreaper", "1" });
                    break;
                // Gun Nut Gang
                case 8754205:
                    ETGModConsole.Spawn(new string[] { "gun_nut", "1" });
                    ETGModConsole.Spawn(new string[] { "chain_gunner", "2" });
                    ETGModConsole.Spawn(new string[] { "spectral_gun_nut", "3" });
                    break;
                // Triple Jamerlengo Bats
                case 8754206:
                    ETGModConsole.Spawn(new string[] { "jamerlengo", "3" });
                    ETGModConsole.Spawn(new string[] { "spirat", "15" });
                    break;
                // Lord of the Jammed
                case 8754207:
                    GameManager.Instance.Dungeon.SpawnCurseReaper();
                    break;
            }
        }

        public static void OnEnemyKill(PlayerController player, HealthHaver enemy)
        {
            if (session == null || !session.Socket.Connected)
            {
                return;
            }
            // There's probably a better way to do this, but I also don't foresee this breaking
            if (enemy.name.Equals("Blobulord(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Blobulord Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("OldBulletKing(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Old King Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("MetalGearRat(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Resourceful Rat Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("Helicopter(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Agunim Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("AdvancedDraGun(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Dragun Killed"] = 1;
                session.DataStorage[Scope.Slot, "Advanced Dragun Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("DraGun(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Dragun Killed"] = 1;
                CheckCompletion();
            }
            if (enemy.name.Equals("Infinilich(Clone)"))
            {
                session.DataStorage[Scope.Slot, "Lich Killed"] = 1;
                CheckCompletion();
            }
        }

        public static void CheckCompletion()
        {
            if (slot_data["Blobulord Goal"] == 1 && !session.DataStorage[Scope.Slot, "Blobulord Killed"])
            {
                return;
            }
            if (slot_data["Old King Goal"] == 1 && !session.DataStorage[Scope.Slot, "Old King Killed"])
            {
                return;
            }
            if (slot_data["Resourceful Rat Goal"] == 1 && !session.DataStorage[Scope.Slot, "Resourceful Rat Killed"])
            {
                return;
            }
            if (slot_data["Agunim Goal"] == 1 && !session.DataStorage[Scope.Slot, "Agunim Killed"])
            {
                return;
            }
            if (slot_data["Advanced Dragun Goal"] == 1 && !session.DataStorage[Scope.Slot, "Advanced Dragun Killed"])
            {
                return;
            }
            if (slot_data["Goal"] == 0 && !session.DataStorage[Scope.Slot, "Dragun Killed"])
            {
                return;
            }
            if (slot_data["Goal"] == 1 && !session.DataStorage[Scope.Slot, "Lich Killed"])
            {
                return;
            }
            // Send Completion
            StatusUpdatePacket packet = new StatusUpdatePacket();
            packet.Status = ArchipelagoClientState.ClientGoal;
            session.Socket.SendPacket(packet);
        }

        public static void Log(string text, string color = "#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }

        public static void OnChestOpen(Chest chest, PlayerController controller)
        {
            if(session != null && session.Socket.Connected)
            {
                chest.contents.Clear();
                chest.ExplodeInSadness();
                session.DataStorage[Scope.Slot, "ChestsOpened"] += 1;
            }
        }

        public static void ArchipelagoConnect(string[] args)
        {
            string ip = args[0];
            string port = args[1];
            string name = args[2];
            string password = null;
            if(args.Length > 3)
            {
                password = args[3];
            }
            if(session != null && session.Socket.Connected)
            {
                Log("You are already connected!", TEXT_COLOR);
                return;
            }
            Log("Connecting To: " + ip + ":" + port + " as " + name);
            allow_traps = false;
            session = ArchipelagoSessionFactory.CreateSession(ip, Int32.Parse(port));
            session.Socket.PacketReceived += OnPacketReceived;
            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;
            // Include own items as we do not want to respawn traps from the starting inventory so we will handle it manually in ArchipelagoItems
            LoginResult result = session.TryConnectAndLogin("Enter The Gungeon", name, ItemsHandlingFlags.IncludeOwnItems, new Version(1, 0, 0), null, null, password, true);
            if(!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to " + ip + ":" + port + " as " + name;
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }
                Log(errorMessage);
                return;
            }
            LoginSuccessful successful_login = (LoginSuccessful)result;
            // Store slot data
            foreach (string key in successful_login.SlotData.Keys)
            {
                //Log(key + " " + successful_login.SlotData[key].ToString());
                int value = int.Parse(successful_login.SlotData[key].ToString());
                slot_data[key] = value;
                
            }
            Log("Connected to Archipelago server.");
            allow_traps = true;
            session.DataStorage[Scope.Slot, "ChestsOpened"].Initialize(0);
            session.DataStorage[Scope.Slot, "Blobulord Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Old King Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Resourceful Rat Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Agunim Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Dragun Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Advanced Dragun Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "Lich Killed"].Initialize(0);
            session.DataStorage[Scope.Slot, "ChestsOpened"].OnValueChanged += (old_value, new_value, _) =>
            {
                for (int i = 0; i < new_value.ToObject<int>(); i++)
                {
                    session.Locations.CompleteLocationChecks(chest_starting_id + i);
                }
            };
        }
        
        public static void ArchipelagoItems()
        {
            if(session == null || !session.Socket.Connected)
            {
                Log("You are not connected!", TEXT_COLOR);
                return;
            }
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                // Don't respawn traps
                if (item.ItemId >= 8754200)
                {
                    continue;
                }
                items_received.Add(item.ItemId);
            }
        }

        public static void OnItemReceived(ReceivedItemsHelper helper)
        {
            ItemInfo info = helper.PeekItem();
            if(allow_traps || !allow_traps && info.ItemId < 8754200)
            {
                items_received.Add(info.ItemId);
            }
            helper.DequeueItem();
        }

        public static void ArchipelagoPickupNotification(string title, string text)
        {
            GameUIRoot.Instance.notificationController.DoCustomNotification(title, text, archipelago_pickup.sprite.collection, archipelago_pickup.sprite.spriteId);
        }

        public static void ArchipelagoChat(string message)
        {
            session.Socket.SendPacketAsync(new SayPacket() { Text = message });
        }

        public static void OnMessageReceived(LogMessage message)
        {
            Log(message.ToString());
        }

        public static void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            if(packet is ItemPrintJsonPacket)
            {
                ItemPrintJsonPacket print = (ItemPrintJsonPacket)packet;
                // Ignore if not one of our items or an item we sent
                if(print.ReceivingPlayer != session.ConnectionInfo.Slot && !session.Locations.AllLocations.Contains(print.Item.Location))
                {
                    return;
                }
                string sending_player_game_name = session.Players.Players[session.ConnectionInfo.Team][print.Item.Player].Game;
                string receiving_player_game_name = session.Players.Players[session.ConnectionInfo.Team][print.ReceivingPlayer].Game;
                ArchipelagoPickupNotification(session.Players.GetPlayerName(print.ReceivingPlayer) + " got " + 
                    session.Items.GetItemName(print.Item.Item, receiving_player_game_name), 
                    "from " + session.Locations.GetLocationNameFromId(print.Item.Location, sending_player_game_name));
            }
        }
    }
}
