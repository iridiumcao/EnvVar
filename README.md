# EnvVar

一个面向 Windows 的环境变量可视化管理工具，使用 WPF 构建。

当前版本实现了文档里定义的 V1 核心能力：读取用户级 / 系统级环境变量、编辑与删除、为变量补充本地元数据，以及对 `PATH` 一类分号分隔值做只读拆分预览。

## 已实现功能

- 浏览用户级与系统级环境变量
- 合并展示或按级别分组展示
- 新建、编辑、删除环境变量
- 编辑本地扩展信息：
  - `Alias`
  - `Description`
- 对包含分号的变量值提供拆分预览
- 写入后广播环境变化，尽量让系统和新进程感知更新

## 本地元数据存储

为了避免污染真实环境变量，`Alias` 与 `Description` 会单独保存在本地 JSON 文件中。

- 路径：`%LocalAppData%\EnvVar\metadata.json`
- 键格式：`Name@Level`

示例：

```json
{
  "JAVA_HOME@User": {
    "alias": "Java Home",
    "description": "JDK installation path"
  }
}
```

## 使用说明

1. 启动应用后，左侧显示环境变量列表，右侧显示详情。
2. 点击任意变量可查看和编辑其内容。
3. 点击“新建”进入创建模式。
4. 点击“保存”写入注册表和本地元数据。
5. 点击“删除”会先进行确认。
6. 若变量值中包含 `;`，右侧会显示拆分预览，但 V1 不支持逐项编辑。

## 权限说明

- 用户级变量通常可直接修改。
- 系统级变量依赖当前进程权限；若没有管理员权限，写入会失败并在界面中明确提示。

## 开发

项目基于 .NET WPF：

```bash
dotnet build
```

入口窗口位于 `MainWindow.xaml`，核心逻辑位于：

- `ViewModels/MainWindowViewModel.cs`
- `Services/EnvironmentVariableService.cs`
- `Services/MetadataStore.cs`

## 当前限制

- 暂不支持 PATH 单项增删改和排序
- 暂不支持搜索、快照、导入导出
- 当前为单窗口 MVP 实现
