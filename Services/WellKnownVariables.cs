namespace EnvVar.Services;

public static class WellKnownVariables
{
    private static readonly Dictionary<string, Dictionary<string, string>> Descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PATH"] = new()
        {
            ["en-US"] = "Directories the system searches for executable files.",
            ["zh-CN"] = "系统搜索可执行文件的目录列表。",
            ["zh-TW"] = "系統搜尋可執行檔的目錄清單。"
        },
        ["TEMP"] = new()
        {
            ["en-US"] = "Directory for temporary files.",
            ["zh-CN"] = "临时文件存放目录。",
            ["zh-TW"] = "暫存檔案存放目錄。"
        },
        ["TMP"] = new()
        {
            ["en-US"] = "Directory for temporary files (alias for TEMP).",
            ["zh-CN"] = "临时文件存放目录（TEMP 的别名）。",
            ["zh-TW"] = "暫存檔案存放目錄（TEMP 的別名）。"
        },
        ["USERPROFILE"] = new()
        {
            ["en-US"] = "Path to the current user's profile directory.",
            ["zh-CN"] = "当前用户配置文件目录的路径。",
            ["zh-TW"] = "目前使用者設定檔目錄的路徑。"
        },
        ["USERNAME"] = new()
        {
            ["en-US"] = "Name of the currently logged-in user.",
            ["zh-CN"] = "当前登录用户的名称。",
            ["zh-TW"] = "目前登入使用者的名稱。"
        },
        ["COMPUTERNAME"] = new()
        {
            ["en-US"] = "NetBIOS name of the computer.",
            ["zh-CN"] = "计算机的 NetBIOS 名称。",
            ["zh-TW"] = "電腦的 NetBIOS 名稱。"
        },
        ["HOMEDRIVE"] = new()
        {
            ["en-US"] = "Drive letter of the user's home directory (e.g. C:).",
            ["zh-CN"] = "用户主目录所在的驱动器号（如 C:）。",
            ["zh-TW"] = "使用者主目錄所在的磁碟機代號（如 C:）。"
        },
        ["HOMEPATH"] = new()
        {
            ["en-US"] = "Path to the user's home directory relative to the home drive.",
            ["zh-CN"] = "用户主目录相对于主驱动器的路径。",
            ["zh-TW"] = "使用者主目錄相對於主磁碟機的路徑。"
        },
        ["APPDATA"] = new()
        {
            ["en-US"] = "Path to the roaming application data directory.",
            ["zh-CN"] = "漫游应用数据目录的路径。",
            ["zh-TW"] = "漫遊應用程式資料目錄的路徑。"
        },
        ["LOCALAPPDATA"] = new()
        {
            ["en-US"] = "Path to the local (non-roaming) application data directory.",
            ["zh-CN"] = "本地（非漫游）应用数据目录的路径。",
            ["zh-TW"] = "本機（非漫遊）應用程式資料目錄的路徑。"
        },
        ["PROGRAMFILES"] = new()
        {
            ["en-US"] = "Directory where 64-bit programs are installed.",
            ["zh-CN"] = "64 位程序的安装目录。",
            ["zh-TW"] = "64 位元程式的安裝目錄。"
        },
        ["PROGRAMFILES(X86)"] = new()
        {
            ["en-US"] = "Directory where 32-bit programs are installed on 64-bit systems.",
            ["zh-CN"] = "64 位系统上 32 位程序的安装目录。",
            ["zh-TW"] = "64 位元系統上 32 位元程式的安裝目錄。"
        },
        ["PROGRAMDATA"] = new()
        {
            ["en-US"] = "Directory for application data shared across all users.",
            ["zh-CN"] = "所有用户共享的应用数据目录。",
            ["zh-TW"] = "所有使用者共用的應用程式資料目錄。"
        },
        ["SYSTEMROOT"] = new()
        {
            ["en-US"] = "Root directory of the Windows system (e.g. C:\\Windows).",
            ["zh-CN"] = "Windows 系统根目录（如 C:\\Windows）。",
            ["zh-TW"] = "Windows 系統根目錄（如 C:\\Windows）。"
        },
        ["WINDIR"] = new()
        {
            ["en-US"] = "Windows installation directory (same as SYSTEMROOT).",
            ["zh-CN"] = "Windows 安装目录（与 SYSTEMROOT 相同）。",
            ["zh-TW"] = "Windows 安裝目錄（與 SYSTEMROOT 相同）。"
        },
        ["SYSTEMDRIVE"] = new()
        {
            ["en-US"] = "Drive letter where Windows is installed (e.g. C:).",
            ["zh-CN"] = "Windows 安装所在的驱动器号（如 C:）。",
            ["zh-TW"] = "Windows 安裝所在的磁碟機代號（如 C:）。"
        },
        ["COMSPEC"] = new()
        {
            ["en-US"] = "Full path to the command-line interpreter (cmd.exe).",
            ["zh-CN"] = "命令行解释器（cmd.exe）的完整路径。",
            ["zh-TW"] = "命令列直譯器（cmd.exe）的完整路徑。"
        },
        ["PATHEXT"] = new()
        {
            ["en-US"] = "File extensions the system recognizes as executable.",
            ["zh-CN"] = "系统识别为可执行文件的扩展名列表。",
            ["zh-TW"] = "系統識別為可執行檔的副檔名清單。"
        },
        ["OS"] = new()
        {
            ["en-US"] = "Operating system identifier.",
            ["zh-CN"] = "操作系统标识符。",
            ["zh-TW"] = "作業系統識別碼。"
        },
        ["PROCESSOR_ARCHITECTURE"] = new()
        {
            ["en-US"] = "CPU architecture (e.g. AMD64, ARM64, x86).",
            ["zh-CN"] = "CPU 架构（如 AMD64、ARM64、x86）。",
            ["zh-TW"] = "CPU 架構（如 AMD64、ARM64、x86）。"
        },
        ["NUMBER_OF_PROCESSORS"] = new()
        {
            ["en-US"] = "Number of logical processors in the system.",
            ["zh-CN"] = "系统中逻辑处理器的数量。",
            ["zh-TW"] = "系統中邏輯處理器的數量。"
        },
        ["PUBLIC"] = new()
        {
            ["en-US"] = "Path to the Public user profile directory.",
            ["zh-CN"] = "Public 用户配置文件目录的路径。",
            ["zh-TW"] = "Public 使用者設定檔目錄的路徑。"
        },
        ["JAVA_HOME"] = new()
        {
            ["en-US"] = "JDK or JRE installation directory.",
            ["zh-CN"] = "JDK 或 JRE 的安装目录。",
            ["zh-TW"] = "JDK 或 JRE 的安裝目錄。"
        },
        ["GOPATH"] = new()
        {
            ["en-US"] = "Go workspace directory.",
            ["zh-CN"] = "Go 工作空间目录。",
            ["zh-TW"] = "Go 工作空間目錄。"
        },
        ["GOROOT"] = new()
        {
            ["en-US"] = "Go SDK installation directory.",
            ["zh-CN"] = "Go SDK 安装目录。",
            ["zh-TW"] = "Go SDK 安裝目錄。"
        },
        ["DOTNET_ROOT"] = new()
        {
            ["en-US"] = ".NET SDK installation directory.",
            ["zh-CN"] = ".NET SDK 安装目录。",
            ["zh-TW"] = ".NET SDK 安裝目錄。"
        },
        ["PYTHONPATH"] = new()
        {
            ["en-US"] = "Additional directories for Python module search.",
            ["zh-CN"] = "Python 模块搜索的附加目录。",
            ["zh-TW"] = "Python 模組搜尋的附加目錄。"
        },
        ["NODE_PATH"] = new()
        {
            ["en-US"] = "Additional directories for Node.js module resolution.",
            ["zh-CN"] = "Node.js 模块解析的附加目录。",
            ["zh-TW"] = "Node.js 模組解析的附加目錄。"
        }
    };

    public static string? GetDescription(string variableName)
    {
        if (!Descriptions.TryGetValue(variableName, out var langMap))
        {
            return null;
        }

        var lang = LocalizationService.CurrentLanguage;
        if (langMap.TryGetValue(lang, out var desc))
        {
            return desc;
        }

        return langMap.TryGetValue("en-US", out var fallback) ? fallback : null;
    }
}
