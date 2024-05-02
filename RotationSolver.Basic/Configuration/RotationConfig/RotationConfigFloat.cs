namespace RotationSolver.Basic.Configuration.RotationConfig;

public class RotationConfigFloat : RotationConfigBase
{
    public float Min, Max, Speed;

    public ConfigUnitType UnitType { get; set; }

    public RotationConfigFloat(string name, object defaultValue, CombatType combatType = CombatType.None, float minimum = 0.0f, float maximum = 1.0f, float speed = 0.02f)
        : base(name, defaultValue, combatType)
    {
        Min = minimum;
        Max = maximum;
        Speed = speed;
    }
}
