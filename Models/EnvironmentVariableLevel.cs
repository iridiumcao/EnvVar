namespace EnvVar.Models;

public enum EnvironmentVariableLevel
{
    User,
    System
}

public static class EnvironmentVariableLevelExtensions
{
    public static EnvironmentVariableTarget ToTarget(this EnvironmentVariableLevel level)
    {
        return level == EnvironmentVariableLevel.User
            ? EnvironmentVariableTarget.User
            : EnvironmentVariableTarget.Machine;
    }
}
