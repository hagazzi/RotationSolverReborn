namespace RotationSolver.Basic.Configuration.RotationConfig;

internal class RotationConfigString(string name, object defaultValue, CombatType combatType = CombatType.None)
    : RotationConfigBase(name, defaultValue, combatType)
{
}
