# 单元测试文档

本项目使用 **xUnit** 作为单元测试框架，结合 **Moq** 进行对象模拟。

## 测试结构

测试项目位于 `EnvVar.Tests/` 目录下，其结构与主项目对应：

- `Utilities/`: 包含对工具类的测试，如 `EnvironmentVariableValueParser`。
- `Models/`: 包含对数据模型的逻辑测试，重点是 `VariableEditorModel`。
- `Services/`: 包含对服务的测试，如 `MetadataStore`（由于部分服务依赖 Windows 注册表，目前主要覆盖逻辑部分）。

## 已覆盖的测试点

### 工具类 (Utilities)
- **EnvironmentVariableValueParser**:
  - 验证多值变量的检测逻辑。
  - 验证分号分隔符的拆分和修剪逻辑。
  - 验证预览字符串的生成（长度控制、换行符处理）。

### 模型 (Models)
- **VariableEditorModel**:
  - 验证从 `EnvironmentVariableEntry` 加载数据的正确性。
  - 验证变更检测逻辑 (`HasChanges`)。
  - 验证值的去重功能 (`DeduplicateValue`)。
  - 验证多值编辑列表的操作（添加、移动、排序）是否正确同步到 `Value` 属性。

### 服务 (Services)
- **MetadataStore**:
  - 验证元数据文件的加载和保存。
  - 验证在文件不存在时的处理逻辑。
  - 验证键名生成逻辑。

## 如何运行测试

您可以通过以下方式运行测试：

### 命令行 (dotnet CLI)

在项目根目录下运行：
```bash
dotnet test
```

### Visual Studio
1. 打开 `Test Explorer` (测试资源管理器)。
2. 点击 `Run All Tests` (运行所有测试)。

## 未来规划

- **Mock 注册表**: 进一步重构 `EnvironmentVariableService` 以支持 Mock 注册表操作，从而实现对环境变量读写的完整测试。
- **UI 测试**: 引入 WPF 相关的 UI 测试工具，验证主界面和设置界面的交互逻辑。
