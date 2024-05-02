namespace RotationSolver.Basic.Configuration.RotationConfig;

public class RotationConfigBoolean(string name, object defaultValue, CombatType combatType = CombatType.None)
    : RotationConfigBase(name, defaultValue, combatType)
{
}
