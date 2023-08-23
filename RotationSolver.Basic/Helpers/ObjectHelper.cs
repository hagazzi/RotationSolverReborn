﻿using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.GeneratedSheets;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Get the information from object.
/// </summary>
public static class ObjectHelper
{
    static readonly EventHandlerType[] _eventType = new EventHandlerType[]
    {
        EventHandlerType.TreasureHuntDirector,
        EventHandlerType.Quest,
    };

    internal static BNpcBase GetObjectNPC(this GameObject obj)
    {
        if (obj == null) return null;
        return Service.GetSheet<BNpcBase>().GetRow(obj.DataId);
    }

    /// <summary>
    /// Is the target have positional.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool HasPositional(this GameObject obj)
    {
        if (obj == null) return false;
        return !(obj.GetObjectNPC()?.Unknown10 ?? false);
    }

    /// <summary>
    /// Is this target belongs to other players.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe bool IsOthersPlayers(this GameObject obj)
    {
        //SpecialType but no NamePlateIcon
        if (_eventType.Contains(obj.GetEventType()))
        {
            return obj.GetNamePlateIcon() == 0;
        }
        return false;
    }

    /// <summary>
    /// Is this target a npc enemy.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe bool IsNPCEnemy(this GameObject obj)
        => obj != null && obj.GetObjectKind() == ObjectKind.BattleNpc
        && ActionManager.CanUseActionOnTarget((uint)ActionID.Blizzard, obj.Struct());

    /// <summary>
    /// Is alliance (can be heal.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe bool IsAlliance(this GameObject obj)
        => obj != null && (obj is PlayerCharacter
        || ActionManager.CanUseActionOnTarget((uint)ActionID.Cure, obj.Struct()));

    /// <summary>
    /// Get the object kind.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe ObjectKind GetObjectKind(this GameObject obj) => (ObjectKind)obj.Struct()->ObjectKind;

    internal static bool IsTopPriorityHostile(this GameObject obj)
    {
        var fateId = DataCenter.FateId;
        //Fate
        if (Service.Config.GetValue(Configuration.PluginConfigBool.TargetFatePriority) &&  fateId != 0 &&  obj.FateId() == fateId) return true;

        var icon = obj.GetNamePlateIcon();

        //Hunting log and weapon.

        if (Service.Config.GetValue(Configuration.PluginConfigBool.TargetHuntingRelicLevePriority) && icon
            is 60092 //Hunting
            or 60096 //Weapon
            or 71244 //Leve
            ) return true;


        if (Service.Config.GetValue(Configuration.PluginConfigBool.TargetQuestPriority) && (icon
            is 71204 //Main Quest
            or 71144 //Major Quest
            or 71224 //Other Quest
            or 71344 //Major Quest
           ||  obj.GetEventType() is EventHandlerType.Quest)) return true;

        return false;
    }

    internal static unsafe uint GetNamePlateIcon(this GameObject obj) => obj.Struct()->NamePlateIconId;
    internal static unsafe void SetNamePlateIcon(this GameObject obj, uint id) => obj.Struct()->NamePlateIconId = id;
    internal static unsafe EventHandlerType GetEventType(this GameObject obj) => obj.Struct()->EventId.Type;

    /// <summary>
    /// The sub kind of the target.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe BattleNpcSubKind GetBattleNPCSubKind(this GameObject obj) => (BattleNpcSubKind)obj.Struct()->SubKind;

    internal static unsafe uint FateId(this GameObject obj) => obj.Struct()->FateId;

    static readonly Dictionary<uint, bool> _effectRangeCheck = new();
    internal static bool CanInterrupt(this BattleChara b)
    {
        var baseCheck = b.IsCasting && b.IsCastInterruptible && b.TotalCastTime >= 2;

        if (!baseCheck) return false;
        if (!Service.Config.GetValue(Configuration.PluginConfigBool.InterruptibleMoreCheck)) return true;

        var id = b.CastActionId;
        if (_effectRangeCheck.TryGetValue(id, out var check)) return check;

        var act = Service.GetSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(b.CastActionId);
        if (act == null) return _effectRangeCheck[id] = false;
        if (act.CastType is 3 or 4) return _effectRangeCheck[id] = false;
        if (act.EffectRange is > 0 and < 8) return _effectRangeCheck[id] = false;
        return _effectRangeCheck[id] = true;
    }

    /// <summary>
    /// Is object a dummy.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsDummy(this BattleChara obj) => obj?.NameId == 541;

    /// <summary>
    /// Is character a boss? Max HP exceeds a certain amount.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsBoss(this BattleChara obj)
    {
        if (obj == null) return false;
        if (obj.IsDummy() && !Service.Config.GetValue(Configuration.PluginConfigBool.ShowTargetDeadTime)) return true;
        return obj.GetDeadTime(true) >= Service.Config.GetValue(Configuration.PluginConfigFloat.DeadTimeBoss)
            || !(obj.GetObjectNPC()?.IsTargetLine ?? true);
    }

    /// <summary>
    /// Is character a dying? Current HP is below a certain amount. It is for running out of resources.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsDying(this BattleChara b)
    {
        if (b == null) return false;
        if (b.IsDummy() && !Service.Config.GetValue(Configuration.PluginConfigBool.ShowTargetDeadTime)) return false;
        return b.GetDeadTime() <= Service.Config.GetValue(Configuration.PluginConfigFloat.DeadTimeDying) || b.GetHealthRatio() < 0.02f;
    }

    private static readonly TimeSpan CheckSpan = TimeSpan.FromSeconds(2.5);

    /// <summary>
    /// How many seconds will the target die.
    /// </summary>
    /// <param name="b"></param>
    /// <param name="wholeTime">whole time to die.</param>
    /// <returns></returns>
    public static float GetDeadTime(this BattleChara b, bool wholeTime = false)
    {
        if (b == null) return float.NaN;
        if (b.IsDummy()) return 999f;

        var objectId = b.ObjectId;

        DateTime startTime = DateTime.MinValue;
        float thatTimeRatio = 0;
        foreach (var (time, hpRatios) in DataCenter.RecordedHP)
        {
            if (hpRatios.TryGetValue(objectId, out var ratio) && ratio != 1)
            {
                startTime = time;
                thatTimeRatio = ratio;
                break;
            }
        }

        var timespan = DateTime.Now - startTime;
        if (startTime == DateTime.MinValue || timespan < CheckSpan) return float.NaN;

        var ratioNow = b.GetHealthRatio();

        var ratioReduce = thatTimeRatio - ratioNow;
        if (ratioReduce <= 0) return float.NaN;

        return (float)timespan.TotalSeconds / ratioReduce * (wholeTime ? 1 : ratioNow);
    }

    /// <summary>
    /// Get the <paramref name="b"/>'s current HP percentage.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float GetHealthRatio(this BattleChara b)
    {
        if (b == null) return 0;
        if (DataCenter.RefinedHP.TryGetValue(b.ObjectId, out var hp)) return hp;
        return (float)b.CurrentHp / b.MaxHp;
    }

    internal static EnemyPositional FindEnemyPositional(this GameObject enemy)
    {
        Vector3 pPosition = enemy.Position;
        float rotation = enemy.Rotation;
        Vector2 faceVec = new((float)Math.Cos(rotation), (float)Math.Sin(rotation));

        Vector3 dir = Player.Object.Position - pPosition;
        Vector2 dirVec = new(dir.Z, dir.X);

        double angle = Math.Acos(Vector2.Dot(dirVec, faceVec) / dirVec.Length() / faceVec.Length());

        if (angle < Math.PI / 4) return EnemyPositional.Front;
        else if (angle > Math.PI * 3 / 4) return EnemyPositional.Rear;
        return EnemyPositional.Flank;
    }

    /// <summary>
    /// The distance from <paramref name="obj"/> to the player
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float DistanceToPlayer(this GameObject obj)
    {
        if (obj == null) return float.MaxValue;
        var player = Player.Object;
        if (player == null) return float.MaxValue;

        var distance = Vector3.Distance(player.Position, obj.Position) - player.HitboxRadius;
        distance -= obj.HitboxRadius;
        return distance;
    }
}
