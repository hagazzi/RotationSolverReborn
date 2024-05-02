using Dalamud.Utility;

namespace RotationSolver.Basic.Configuration.RotationConfig;

public class RotationConfigCombo: RotationConfigBase
{
    public string[] DisplayValues { get; }

    public RotationConfigCombo(string name, object defaultValue, CombatType combatType = CombatType.None)
        :base(name, defaultValue, combatType)
    {
        var names = new List<string>();
        foreach (Enum v in Enum.GetValues(defaultValue.GetType()))
        {
            names.Add(v.GetAttribute<DescriptionAttribute>()?.Description ?? v.ToString());
        }

        DisplayValues = [.. names];
    }

    public override string ToString()
    {
        var indexStr = base.ToString();
        if (!int.TryParse(indexStr, out var index)) return DisplayValues[0].ToString();
        return DisplayValues[index];
    }
}
