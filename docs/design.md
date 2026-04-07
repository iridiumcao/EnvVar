# EnvVar 功能设计文档

## 1. 项目目标

开发一个 Windows 下的环境变量可视化管理工具，
提供 CRUD、信息增强、多值变量结构化编辑、导入导出与单变量历史恢复能力，
提升开发者对环境变量的可读性与可操作性。

---

## 2. 功能说明

### 2.1 环境变量 CRUD

支持以下操作：

- 查看环境变量列表
- 新增环境变量
- 编辑环境变量（name / value / level）
- 删除环境变量

支持变量级别：

- User（用户级）
- System（系统级）

---

### 2.2 数据增强（本地扩展信息）

为每个环境变量提供附加信息（仅存本地 JSON 文件）：

- Alias（用户自定义名称）
- Description（备注说明）

对于常见的系统环境变量（如 PATH、TEMP、USERPROFILE 等），软件内置了预设描述，支持三种语言。
如果用户未自定义描述，预设描述会随软件语言设置实时切换。
用户一旦手动编辑并保存描述，该描述将作为自定义信息存储，不再随语言切换。

存储路径：`%LocalAppData%\EnvVar\metadata.json`

键格式：`Name@Level`（复合主键，避免 User / System 同名冲突）

示例：

```json
{
  "JAVA_HOME@User": {
    "alias": "Java Home",
    "description": "JDK installation path"
  }
}
```

---

### 2.3 展示模式

#### 分级展示（Grouped，默认）

- 按 User / System 分组显示
- 每组内按名称排序

#### 合并展示（Merged）

- 所有变量合并为一个列表
- 每个变量标记 Level
- 默认按 Level + Name 排序

列表支持按列排序（Name / Alias / Level / Preview），三次点击循环：升序 → 降序 → 重置。

---

### 2.4 多值变量结构化编辑

适用于 PATH、CLASSPATH 等以分号（`;`）分隔的变量。

功能：

- 自动识别分号分隔结构并拆分为列表展示
- 支持逐项编辑：直接在列表中修改单个值
- 支持添加 / 删除 / 上移 / 下移单项
- 支持按字母升序 / 降序排列所有项
- 编辑后自动同步回原始 Value 字符串
- **自动去重**：保存变量时，系统会自动检测并移除多值变量（以分号分隔）中的重复项，并忽略空值，确保路径等信息的唯一性

示例：

```plaintext
PATH = C:\Java\bin;C:\Windows\System32
```

→ 展示为：

```plaintext
[0] C:\Java\bin
[1] C:\Windows\System32
```

---

### 2.5 搜索

左侧列表顶部提供搜索框（带放大镜图标暗示），支持按 Name、Alias、Value 实时过滤。

---

### 2.6 导入 / 导出

- **导出**：将所有环境变量（含元数据）导出为 JSON 文件
- **导入**：从 JSON 文件读取变量列表，确认后写入，同名变量被覆盖

---

### 2.8 主题支持 (深色模式)

- 支持浅色 (Light)、深色 (Dark) 以及跟随系统 (System) 三种模式
- 通过切换 ResourceDictionary 实现动态换肤
- 模式选择持久化存储在 `settings.json` 中
- 跟随系统模式下，实时监听 Windows 注册表主题变化并自动切换

### 2.10 未保存修改提醒

- 在以下操作前自动检查当前变量是否有未保存的修改：
    - 切换选中的环境变量
    - 点击「新建」按钮
    - 点击「刷新」按钮
    - 关闭应用程序窗口
- 如果检测到修改，弹出确认对话框（保存 / 不保存 / 取消）
- 通过对比当前编辑器内容与原始加载内容实现修改检测

### 2.11 设置管理

- 统一管理语言、历史保留份数、主题等配置
- 存储在 `%LocalAppData%\EnvVar\settings.json`

---

### 2.8 多语言

支持三种界面语言：

- English（默认）
- 简体中文
- 繁體中文

语言选择会持久化到 `%LocalAppData%\EnvVar\language.txt`，下次启动自动加载。

---

## 3. 非功能需求

### 3.1 权限控制

- 修改 System 变量时需管理员权限
- 权限不足时提示以管理员身份重启应用

### 3.2 数据安全

- 修改前读取最新系统变量
- 删除 / 覆盖前均需用户确认
- 保存和删除操作前自动记录该变量的历史版本（包含 Value、Alias、Description）

### 3.3 性能

- 启动时间 < 1 秒
- 支持变量数量：100+

### 3.4 可用性

- 经典「列表 + 详情面板」单页面布局
- 操作路径清晰，关键操作均有确认

---

## 4. 技术实现

### 4.1 系统读取

来源：

- User：`HKCU\Environment`
- System：`HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`

### 4.2 更新机制

- 写入 Registry
- 广播 `WM_SETTINGCHANGE` 通知系统刷新

### 4.3 多值解析

- 分隔符：`;`
- 去除空项，保留原始顺序

### 4.4 技术栈

- .NET 10 / WPF
- 单项目结构
- MVVM 模式（手动实现 `ObservableObject`）
