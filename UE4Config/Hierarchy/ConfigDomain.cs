﻿namespace UE4Config.Hierarchy
{
    /// <summary>
    /// The configuration domain identifies where a configuration is stored and modified.
    /// </summary>
    public enum ConfigDomain
    {
        Custom,
        /// <example>
        /// Engine/Base.ini
        /// "{ENGINE}/Config/Base.ini"
        /// </example>
        /// <example>
        /// Engine/Base*.ini
        /// "{ENGINE}/Config/Base{TYPE}.ini"
        /// </example>
        /// <example>
        /// Engine/Platform/BasePlatform*.ini
        /// "{ENGINE}/Config/{PLATFORM}/Base{PLATFORM}{TYPE}.ini"
        /// </example>
        EngineBase,
        /// <example>
        /// Engine/Platform/Platform*.ini
        /// "{ENGINE}/Config/{PLATFORM}/{PLATFORM}{TYPE}.ini"
        /// </example>
        Engine,
        /// <example>
        /// Project/Default*.ini
        /// "{PROJECT}/Config/Default{TYPE}.ini"
        /// </example>
        /// <example>
        /// Project/Default*.ini
        /// "{PROJECT}/Config/Default{TYPE}.ini"
        /// </example>
        /// <example>
        /// Project/Platform/Platform*.ini
        /// "{PROJECT}/Config/{PLATFORM}/{PLATFORM}{TYPE}.ini"
        /// </example>
        Project,
        /// <summary>
        /// Project config files which are generated by buildmachine processes (i.e. should never be checked in)
        /// </summary>
        /// <example>
        /// Project/Platform/Generated*.ini
        /// {PROJECT}/Config/Generated{TYPE}.ini
        /// </example>
        /// <example>
        /// Project/Platform/GeneratedPlatform*.ini
        /// {PROJECT}/Config/{PLATFORM}/Generated{PLATFORM}{TYPE}.ini
        /// </example>
        ProjectGenerated,
        /*
         TODO Support user level layers
        /// <example>
        /// {USERSETTINGS}/Unreal Engine/Engine/Config/User{TYPE}.ini
        /// UserSettings/.../User*.ini
        /// </example>
        UserSettings,
        /// <summary>
        /// Global user settings
        /// </summary>
        /// <example>
        /// {USER}/Unreal Engine/Engine/Config/User{TYPE}.ini
        /// UserDir/.../User*.ini
        /// </example>
        GlobalUser,
        /// <summary>
        /// User project overrides
        /// </summary>
        /// <example>
        /// {PROJECT}/User{TYPE}.ini
        /// Project/User*.ini
        /// </example>
        ProjectUser,
        */
    }
}

