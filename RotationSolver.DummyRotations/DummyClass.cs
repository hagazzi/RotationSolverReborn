using ImGuiNET;
using RotationSolver.Basic.Actions;
using RotationSolver.Basic.Attributes;
using RotationSolver.Basic.Configuration.RotationConfig;
using RotationSolver.Basic.Data;
using RotationSolver.Basic.Rotations.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RotationSolver.DummyRotations
{
    [Rotation("Dummy Rotation", CombatType.PvE, GameVersion = "6.58")]
    [Api(1)]
    public class DummyClass : MachinistRotation
    {
        public enum Order123Enum
        {
            Order1,
            Order2,
            Order3
        }
        public RotationConfigBoolean UseTactician { get; set; } = new RotationConfigBoolean("Use Tactician", true, CombatType.PvE);
        public RotationConfigCombo Order123 { get; set; } = new RotationConfigCombo("Order 1-2-3", Order123Enum.Order1, CombatType.PvE);
        protected override bool GeneralGCD(out IAction? act)
        {
            return base.GeneralGCD(out act);
        }

        protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
        {
            return base.GeneralAbility(nextGCD, out act);
        }

        protected override IAction? CountDownAction(float remainTime)
        {
            return base.CountDownAction(remainTime);
        }

        protected override bool AttackAbility(IAction nextGCD, out IAction? act)
        {
            return base.AttackAbility(nextGCD, out act);
        }

        protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        public override void DisplayStatus()
        {
            ImGui.Text($"Use Tactician: {UseTactician}");
            ImGui.Text($"Order 1-2-3: {Order123}");
        }
    }
}
