# EnvVar 建议与改进方向

基于对当前代码的审阅，以下是我对项目的一些改进建议，按优先级分类。

---

## 一、代码质量与架构

### 1. 引入 MVVM 命令框架

当前所有按钮事件（`SaveButton_OnClick`、`DeleteButton_OnClick` 等）都写在 `MainWindow.xaml.cs` 的 code-behind 中，导致 View 层承担了较多的流程控制逻辑（如弹窗确认、异常处理）。

**建议**：引入 `ICommand` / `RelayCommand`，将按钮操作绑定到 ViewModel 的命令属性上。
这样做的好处：

- View 层更薄，仅负责 UI 交互
- ViewModel 可独立进行单元测试
- 可以更自然地控制按钮的 `CanExecute` 状态

### 2. 抽象弹窗交互

当前 ViewModel 无法控制弹窗（确认对话框、文件选择器），这些都直接写在 code-behind 中。

**建议**：定义一个 `IDialogService` 接口：

```csharp
public interface IDialogService
{
    bool Confirm(string message, string title);
    string? ShowSaveFileDialog(string title, string filter, string defaultName);
    string? ShowOpenFileDialog(string title, string filter);
}
```

在 code-behind 中实现并注入 ViewModel，这样 ViewModel 可以完整处理业务流程而无需依赖 WPF 类型。

### 3. 引入依赖注入

当前 `MainWindowViewModel` 在构造函数中直接 `new` 了 `EnvironmentVariableService`、`ExportImportService`、`VersionHistoryService`。

**建议**：使用 `Microsoft.Extensions.DependencyInjection`，在 `App.xaml.cs` 中配置服务容器。好处：

- 服务生命周期统一管理
- 方便测试时替换实现
- 为将来拆分功能做准备

---

## 二、功能增强

### 4. 变量比较与差异查看

开发者经常需要对比两台机器或两次快照的环境变量差异。

**建议**：增加差异对比功能：

- 选中一个快照与当前环境对比
- 高亮新增 / 修改 / 删除的变量

### 5. 变量值校验

对于 PATH 类变量，包含的路径可能实际不存在。

**建议**：在结构化编辑区为每一项增加校验提示：

- 路径不存在时标红或显示警告图标
- 检测到重复项时给出提示

---

## 三、用户体验

### 6. 键盘快捷键

当前操作全部依赖鼠标。

**建议**：为常用操作添加快捷键：

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+S` | 保存 |
| `Ctrl+N` | 新建 |
| `F5` | 刷新 |
| `Delete` | 删除（需确认） |
| `Ctrl+F` | 聚焦搜索框 |
| `Escape` | 取消编辑 |

### 7. 结构化编辑区拖拽排序

当前多值项的排序依赖上移 / 下移按钮。

**建议**：支持鼠标拖拽重排（ListBox 拖放），操作更直观。

### 9. 未保存修改提醒

当前切换选中变量或关闭窗口时不会检查是否有未保存的修改。

**建议**：检测编辑器内容是否被修改，若有未保存变更，在切换或退出时弹窗提醒。

---

## 四、工程化

### 8. 单元测试

当前项目没有测试项目。`VariableEditorModel`、`EnvironmentVariableValueParser`、`MetadataStore` 等都有明确的输入输出，非常适合单元测试。

**建议**：新建 `EnvVar.Tests` 项目，优先覆盖：

- `EnvironmentVariableValueParser`（拆分 / 合并逻辑）
- `VariableEditorModel`（EditableValues 同步逻辑）
- `MetadataStore`（JSON 序列化 / 反序列化）
- `ExportImportService`（导入导出）

### 9. 日志

当前异常仅在状态栏和弹窗中展示，没有持久化的日志。

**建议**：引入 `Microsoft.Extensions.Logging` 并写入文件日志，方便排查用户反馈的问题。

---

## 五、小改进

- `SnapshotInfo` 和 `ExportData` 等小类目前定义在 Service 文件尾部，建议单独放到 `Models/` 目录
- `VersionHistoryService` 构造函数中直接 `Directory.CreateDirectory`，若权限不足可能抛异常，建议加 try-catch
- `EditableValueItem` 的 `PropertyChanged` 订阅 / 取消订阅较分散，可考虑封装为一个方法统一管理
