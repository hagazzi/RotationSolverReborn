namespace RotationSolver.Basic.Configuration.RotationConfig;

internal class RotationConfigInt : RotationConfigBase
{
    public int Min, Max, Speed;

    public RotationConfigInt(string name, object defaultValue, CombatType combatType = CombatType.None, int minimum = 0, int maximum = 10, int speed = 1)
        : base(name, defaultValue, combatType)
    {
        Min = minimum;
        Max = maximum;
        Speed = speed;
    }
}
