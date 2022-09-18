using System;
using System.Collections.Generic;
using Hazel;
using InnerNet;
using UnityEngine;

namespace TownOfHost
{
    //参考元 : https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Mode/SuperHostRoles/BlockTool.cs
    class DisableDevice
    {
        private static List<byte> OldDesyncCommsPlayers = new();
        private static int Count = 0;
        public static float UsableDistance()
        {
            var Map = (MapNames)PlayerControl.GameOptions.MapId;
            return Map switch
            {
                MapNames.Skeld => 1.5f,
                MapNames.Mira => 2.2f,
                MapNames.Polus => 1.5f,
                //MapNames.Dleks => 1.5f,
                MapNames.Airship => 1.5f,
                _ => 0.0f
            };
        }
        public static void FixedUpdate()
        {
            Count--;
            if (Count > 0) return;
            Count = 3;
            var DisableDevices =
                AdminPatch.DisableAdmin ||
                Options.IsStandardHAS; //他に無効化するデバイスを設定する場合はここへ追加

            if (DisableDevices)
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    try
                    {
                        if (!pc.IsModClient())
                        {
                            var clientId = pc.GetClientId();
                            bool IsGuard = false;
                            Vector2 PlayerPos = pc.GetTruePosition();
                            //アドミンチェック
                            if ((AdminPatch.DisableAdmin || Options.IsStandardHAS) && pc.IsAlive())
                            {
                                float distance;
                                switch (PlayerControl.GameOptions.MapId)
                                {
                                    case 0:
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["SkeldAdmin"]);
                                        IsGuard = distance <= UsableDistance();
                                        break;
                                    case 1:
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["MiraHQAdmin"]);
                                        IsGuard = distance <= UsableDistance();
                                        break;
                                    case 2:
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["PolusLeftAdmin"]);
                                        IsGuard = distance <= UsableDistance();
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["PolusRightAdmin"]);
                                        IsGuard = distance <= UsableDistance() || IsGuard;
                                        break;
                                    case 4:
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["AirshipCockpitAdmin"]);
                                        IsGuard = distance <= UsableDistance();
                                        distance = Vector2.Distance(PlayerPos, AdminPatch.AdminPos["AirshipRecordsAdmin"]);
                                        IsGuard = distance <= UsableDistance();
                                        break;
                                }
                            }
                            if (IsGuard && !pc.inVent)
                            {
                                if (!OldDesyncCommsPlayers.Contains(pc.PlayerId))
                                    OldDesyncCommsPlayers.Add(pc.PlayerId);

                                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                                SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                                SabotageFixWriter.Write((byte)128);
                                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                            }
                            else
                            {
                                if (!Utils.IsActive(SystemTypes.Comms) && OldDesyncCommsPlayers.Contains(pc.PlayerId))
                                {
                                    OldDesyncCommsPlayers.Remove(pc.PlayerId);

                                    /*var sender = CustomRpcSender.Create("DisableDevice", SendOption.Reliable);

                                    sender.AutoStartRpc(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, clientId)
                                            .Write((byte)SystemTypes.Comms)
                                            .WriteNetObject(pc)
                                            .Write((byte)16)
                                            .EndRpc();
                                    if (PlayerControl.GameOptions.MapId == 2)
                                        sender.AutoStartRpc(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, clientId)
                                                .Write((byte)SystemTypes.Comms)
                                                .WriteNetObject(pc)
                                                .Write((byte)17)
                                                .EndRpc();

                                    sender.SendMessage();*/

                                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                                    SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                                    SabotageFixWriter.Write((byte)16);
                                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString(), "DeviceBlock");
                    }
                }
            }
        }
        public static Vector2 GetAdminTransform()
        {
            var MapName = (MapNames)PlayerControl.GameOptions.MapId;
            return MapName switch
            {
                MapNames.Skeld => new Vector2(3.48f, -8.624401f),
                //MapNames.Mira => new Vector2(20.524f, 20.595f),
                MapNames.Polus => new Vector2(22.13707f, -21.523f),
                //MapNames.Dleks => new Vector2(-3.48f, -8.624401f),
                MapNames.Airship => new Vector2(-22.323f, 0.9099998f),
                _ => new Vector2(1000, 1000)
            };
        }
    }
}