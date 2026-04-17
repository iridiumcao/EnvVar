# EnvVar 建议与改进方向

本文档记录了对 EnvVar 项目的改进建议，旨在提升代码质量、用户体验及功能的完备性。

---

## 一、 架构与代码质量 (优先级：高)

### 1. 深度落实 MVVM 模式
目前虽然存在 `MainWindowViewModel`，但大量的逻辑（弹窗、排序控制、窗口状态管理）仍滞留在 `MainWindow.xaml.cs` 中。
- **建议**：引入 `RelayCommand` 或利用社区成熟的 MVVM 库（如 `CommunityToolkit.Mvvm`），将 code-behind 中的点击事件全面迁移至 ViewModel。
- **目标**：实现 View 与 Logic 的彻底解耦。

### 2. 引入依赖注入 (DI)
当前 Service 类的实例化散落在各处（如 `App.xaml.cs` 和各 ViewModel 构造函数），不利于管理生命周期。
- **建议**：使用 `Microsoft.Extensions.DependencyInjection` 统一管理 `EnvironmentVariableService`、`LoggingService` 等。
- **目标**：增强代码的可维护性和模块化。

### 3. 抽象弹窗与交互服务 (IDialogService)
ViewModel 目前无法直接触发确认框或错误提示。
- **建议**：定义并实现 `IDialogService` 接口，将 `ThemedMessageBox` 和 `SaveFileDialog` 的调用封装其中。
- **目标**：使 ViewModel 的业务流程可被单元测试覆盖。

### 4. 提升可测试性：引入接口抽象
`EnvironmentVariableService` 直接操作 Windows 注册表，导致难以编写不依赖环境的单元测试。
- **建议**：定义 `IRegistryProvider` 接口，在生产环境使用注册表，在测试环境使用 Mock 实现。
- **目标**：实现核心业务逻辑的 100% 覆盖测试。

---

## 二、 功能增强 (优先级：中)

### 5. PATH 路径有效性校验 (已识别需求)
用户在编辑 PATH 变量时，可能会输入不存在的路径。
- **建议**：在多值编辑列表中，实时检测每一行路径的物理存在性。若不存在，显示警告图标（UI 细节可见 `ui-design.md`）。

### 6. 环境快照对比 (Diff)
方便用户对比当前环境与历史快照（导入的文件）的差异。
- **建议**：增加一个简单的对比模式，标记出哪些变量被修改了。

### 7. 增强的日志查看器
虽然已有日志记录，但查看日志仍需用户手动打开 LocalAppData 目录。
- **建议**：在「关于」或「设置」页面增加「打开日志目录」或「实时日志流查看」按钮。

---

## 三、 用户体验优化 (优先级：低)

### 8. 快捷键支持
当前软件对键盘用户不够友好。
- **建议**：支持以下常用快捷键：
  - `Ctrl + S`: 保存当前修改
  - `Ctrl + N`: 新建变量
  - `F5`: 刷新列表
  - `Ctrl + F`: 聚焦搜索框

### 9. 多值项拖拽排序
目前 PATH 项的排序依赖点击「上移/下移」按钮，效率较低。
- **建议**：支持鼠标在列表项上直接拖拽重排序。

### 10. 搜索功能增强
目前的搜索是全匹配或简单包含。
- **建议**：支持正则表达式搜索，或对搜索关键字进行多字段（Name, Value, Alias）加权匹配。
