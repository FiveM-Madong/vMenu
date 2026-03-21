using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new();
        public Dictionary<int, int> PlayerCoordWaypoints = new();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        readonly Menu playerMenu = new("접속중인 유저", "유저 이름:");
        IPlayer currentPlayer = new NativePlayer(Game.Player);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Online Players")
            {
                CounterPreText = "해당 유저 이름: "
            };

            MenuController.AddSubmenu(menu, playerMenu);

            var sendMessage = new MenuItem("개인 메세지 보내기", "이 플레이어에게 개인 메세지를 보냅니다. ~r~참고: 관리자(스태프)는 모든 개인 메세지를 확인할 수 있습니다.");
            var teleport = new MenuItem("유저에게 텔레포트", "이 플레이어가 있는 위치로 이동합니다.");
            var teleportVeh = new MenuItem("유저차량에 텔레포트", "이 플레이어가 타고 있는 차량으로 이동합니다.");
            var summon = new MenuItem("나에게 텔레포트", "이 플레이어를 내 위치로 이동시킵니다.");
            var toggleGPS = new MenuItem("실시간 유저추적", "이 플레이어까지의 GPS 경로 표시를 레이더에 켜거나 끕니다.");
            var spectate = new MenuItem("해당 유저 관전하기", "이 플레이어를 관전합니다. 다시 클릭하면 관전을 중지합니다.");
            var printIdentifiers = new MenuItem("해당 유저 조사하기", "이 플레이어의 식별자 정보를 클라이언트 콘솔(F8)에 출력하고, CitizenFX.log 파일에도 저장합니다.");
            var kill = new MenuItem("~r~유저 죽이기", "이 플레이어를 사망시킵니다. 해당 플레이어에게는 당신이 죽였다는 알림이 표시되며, 관리자 작업 로그에도 기록됩니다.");
            var kick = new MenuItem("~r~강제 퇴장", "이 플레이어를 서버에서 강제 퇴장시킵니다.");
            var ban = new MenuItem("~r~영구 정지", "이 플레이어를 서버에서 영구적으로 차단합니다. 정말 진행하시겠습니까? 차단사유 입력가능");
            var tempban = new MenuItem("~r~일시 정지", "이 플레이어를 최대 30일까지 일시 차단합니다. 버튼을 누른 뒤 차단 기간과 사유를 입력할 수 있습니다.");


            if (IsAllowed(Permission.OPSendMessage))
            {
                playerMenu.AddMenuItem(sendMessage);
            }
            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddMenuItem(teleport);
                playerMenu.AddMenuItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddMenuItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddMenuItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddMenuItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddMenuItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddMenuItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddMenuItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddMenuItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddMenuItem(ban);
                ban.LeftIcon = MenuItem.Icon.WARNING;
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                ban.Label = "";
            };

            playerMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                ban.Label = "";
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += async (sender, item, index) =>
            {
                // send message
                if (item == sendMessage)
                {
                    if (currentPlayer.Handle == Game.Player.Handle)
                    {
                        Notify.Error("자기자신에겐 보낼 수 없습니다!");
                        return;
                    }

                    if (MainMenu.MiscSettingsMenu != null && !MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
                    {
                        var message = await GetUserInput($"{currentPlayer.Name}에게 개인 메세지 보내기", 200);
                        if (string.IsNullOrEmpty(message))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            TriggerServerEvent("메세지를 보냈습니다.", currentPlayer.ServerId, message);
                            PrivateMessage(currentPlayer.ServerId.ToString(), message, true);
                        }
                    }
                    else
                    {
                        Notify.Error("본인의 개인 메세지 기능이 비활성화되어 있으면 개인 메세지를 보낼 수 없습니다. Misc 설정 메뉴에서 해당 기능을 활성화한 뒤 다시 시도하세요.");
                    }

                }
                // teleport (in vehicle) button
                else if (item == teleport || item == teleportVeh)
                {
                    if (!currentPlayer.IsLocal)
                    {
                        _ = TeleportToPlayer(currentPlayer, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    }
                    else
                    {
                        Notify.Error("자기자신에게 텔레포트는 할 수 없습니다.");
                    }
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                    {
                        SummonPlayer(currentPlayer);
                    }
                    else
                    {
                        Notify.Error("자기자신에게 텔레포트는 할 수 없습니다.");
                    }
                }
                // spectating
                else if (item == spectate)
                {
                    SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    KillPlayer(currentPlayer);
                }
                // manage the gps route being clicked.
                else if (item == toggleGPS)
                {
                    var selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.ServerId))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (var serverId in PlayersWaypointList)
                        {
                            // remove any coord blip
                            if (PlayerCoordWaypoints.TryGetValue(serverId, out var wp))
                            {
                                SetBlipRoute(wp, false);
                                RemoveBlip(ref wp);

                                PlayerCoordWaypoints.Remove(serverId);
                            }

                            // remove any entity blip
                            var playerId = GetPlayerFromServerId(serverId);

                            if (playerId < 0)
                            {
                                continue;
                            }

                            var playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                var oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~s~ 플레이어에 대한 GPS 경로 안내가 비활성화되었습니다.");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.ServerId != Game.Player.ServerId)
                        {
                            int blip;

                            if (currentPlayer.IsActive && currentPlayer.Character != null)
                            {
                                var ped = GetPlayerPed(currentPlayer.Handle);
                                blip = GetBlipFromEntity(ped);
                                if (!DoesBlipExist(blip))
                                {
                                    blip = AddBlipForEntity(ped);
                                }
                            }
                            else
                            {
                                if (!PlayerCoordWaypoints.TryGetValue(currentPlayer.ServerId, out blip))
                                {
                                    var coords = await MainMenu.RequestPlayerCoordinates(currentPlayer.ServerId);
                                    blip = AddBlipForCoord(coords.X, coords.Y, coords.Z);
                                    PlayerCoordWaypoints[currentPlayer.ServerId] = blip;
                                }
                            }

                            SetBlipColour(blip, 58);
                            SetBlipRouteColour(blip, 58);
                            SetBlipRoute(blip, true);

                            PlayersWaypointList.Add(currentPlayer.ServerId);
                            Notify.Custom($"~g~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~s~ 플레이어에 대한 GPS 경로 안내가 활성화되었습니다. 경로 안내를 끄려면 ~s~GPS 경로 전환~g~ 버튼을 다시 누르세요.");
                        }
                        else
                        {
                            Notify.Error("You can not set a waypoint to yourself.");
                        }
                    }
                }
                else if (item == printIdentifiers)
                {
                    // TODO: Replace callback function
                    Func<string, string> CallbackFunction = (data) =>
                    {
                        Debug.WriteLine(data);
                        var ids = "~s~";
                        foreach (var s in JsonConvert.DeserializeObject<string[]>(data))
                        {
                            ids += "~n~" + s;
                        }
                        Notify.Custom($"~y~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~'s Identifiers: {ids}", false);
                        return data;
                    };
                    BaseScript.TriggerServerEvent("vMenu:GetPlayerIdentifiers", currentPlayer.ServerId, CallbackFunction);
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                    {
                        KickPlayer(currentPlayer, true);
                    }
                    else
                    {
                        Notify.Error("You cannot kick yourself!");
                    }
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.Label == "Are you sure?")
                    {
                        ban.Label = "";
                        _ = UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.Label = "Are you sure?";
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
                {
                    var baseId = int.Parse(item.Label.Replace(" →→→", "").Replace("Server #", ""));
                    var player = MainMenu.PlayersList.FirstOrDefault(p => p.ServerId == baseId);

                    if (player != null)
                    {
                        currentPlayer = player;
                        playerMenu.MenuSubtitle = $"~s~Player: ~y~{GetSafePlayerName(currentPlayer.Name)}";
                        playerMenu.CounterPreText = $"[Server ID: ~y~{currentPlayer.ServerId}~s~] ";
                    }
                    else
                    {
                        playerMenu.GoBack();
                    }
                };
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public async Task UpdatePlayerlist()
        {
            void UpdateStuff()
            {
                menu.ClearMenuItems();

                foreach (var p in MainMenu.PlayersList.OrderBy(a => a.Name))
                {
                    var pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"이 플레이어의 옵션을 보려면 클릭하세요 Server ID: {p.ServerId}. Local ID: {p.Handle}.")
                    {
                        Label = $"Server #{p.ServerId} →→→"
                    };
                    menu.AddMenuItem(pItem);
                    MenuController.BindMenuItem(menu, playerMenu, pItem);
                }

                menu.RefreshIndex();
                //menu.UpdateScaleform();
                playerMenu.RefreshIndex();
                //playerMenu.UpdateScaleform();
            }

            // First, update *before* waiting - so we get all local players.
            UpdateStuff();
            await MainMenu.PlayersList.WaitRequested();

            // Update after waiting too so we have all remote players.
            UpdateStuff();
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                _ = UpdatePlayerlist();
                return menu;
            }
        }
    }
}
