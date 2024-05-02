using ECommons.DalamudServices;

namespace RotationSolver.Basic.Configuration.RotationConfig;

public abstract class RotationConfigBase
    : IRotationConfig
{
    readonly PropertyInfo _property;
    readonly ICustomRotation _rotation;
    public string Name { get; }
    public string DefaultValue { get; }
    public string DisplayName { get; }
    public CombatType Type { get; }

    public string Value 
    {
        get
        {
            if (!Service.Config.RotationConfigurations.TryGetValue(Name, out var config)) return DefaultValue;
            return config;
        }
        set
        {
            Service.Config.RotationConfigurations[Name] = value;
            SetValue(value);
        }
    }

    protected RotationConfigBase(string name, object defaultValue, CombatType combatType = CombatType.None)
    {
        Name = name;
        DefaultValue = defaultValue.ToString();
        DisplayName = name;
        Type = combatType;

        //Set Up
        if (Service.Config.RotationConfigurations.TryGetValue(Name, out var value))
        {
            SetValue(value);
        }
    }

    private void SetValue(string value)
    {
        var type = _property.PropertyType;
        if (type == null) return;

        try
        {
            _property.SetValue(_rotation, ChangeType(value, type));
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Failed to convert type.");
            _property.SetValue(_rotation, ChangeType(DefaultValue, type));
        }
    }

    private static object ChangeType(string value, Type type)
    {
        if (type.IsEnum)
        {
            return Enum.Parse(type, value);
        }
        else if(type == typeof(bool))
        {
            return bool.Parse(value);
        }

        return Convert.ChangeType(value, type);
    }

    public override string ToString() => Value;
}
