# Custom Card Balance：维护流程

## 1. 目标

本文档固化 Ver0.106 系列 `Custom Card Balance` 的 Excel 驱动更新流程。后续用户要求“更新 Mod”时，必须先读取完整 Excel，再和当前 Mod 对比，输出确认表；只有收到明确“开始工作”指令后，才能修改代码和部署。

## 2. Excel 输入

当前工作簿位于：

`<WORKSPACE>\outputs\revertcardsmod-menu-content\RevertCardsMod_Menu_Content-2.3.0.xlsx`

历史文件名中的 `RevertCardsMod` 只表示维护输入来源，不代表当前 Mod 名称或运行时 ID。

Excel 固定包含 6 个标签页：

1. 铁甲战士
2. 静默猎手
3. 故障机器人
4. 储君
5. 亡灵契约师
6. 其他

Excel 不创建“全部”标签页。游戏设置面板中的“全部”由程序按以上顺序合并生成。

每个标签页固定 5 列：

| 列名 | 用途 |
| --- | --- |
| 卡牌名称 | 游戏中的中文卡牌名称 |
| 调整内容 | 设置面板中的调整摘要 |
| 升级前效果 | 计划应用的基础卡牌效果文本 |
| 升级后效果 | 计划应用的升级卡牌效果文本 |
| 开关 | 新增卡牌的默认开关状态 |

Excel 是维护输入和 UI 文案来源，但不能替代源码分析。实现卡牌前仍然必须读取目标游戏版本的解包源码。

## 3. 阶段一：读取与确认

当用户要求更新 Mod 时，先执行以下步骤：

1. 确认目标游戏版本和解包源码目录。
2. 读取 Excel 全部 6 个标签页，不得只读新增行。
3. 读取当前 Mod 的 `ModConfiguration.cs`、`SettingsPanel.cs`、`CardPatches.cs`、`CustomCardBalance.json`。
4. 对比 Excel 与当前 Mod，分类为：
   - Excel 中新增、当前 Mod 未包含
   - 已有卡牌但 Excel 文案或逻辑变化
   - 已有卡牌且一致
   - 当前 Mod 存在但 Excel 缺失
5. 对每张新增或变化卡牌读取目标游戏版本源码，核对类名、费用、稀有度、动态变量、关键字、`OnPlay`、`OnUpgrade`、描述和悬浮信息。
6. 输出对比表给用户确认。

对比表至少包含：

| 列名 | 内容 |
| --- | --- |
| 所属分类 | 角色标签页 |
| 卡牌名称 | 中文名 |
| 内部类名或 ID | 源码识别信息 |
| 当前游戏版本升级前效果 | 目标版本原版 |
| 当前游戏版本升级后效果 | 目标版本原版 |
| Excel 计划升级前效果 | 用户维护内容 |
| Excel 计划升级后效果 | 用户维护内容 |
| 调整内容摘要 | Excel 文案 |
| 预计实现位置 | 费用、变量、关键字、升级、打出效果、描述等 |
| 待确认问题 | 源码与 Excel 无法直接对齐的地方 |

输出对比表后停止，等待用户确认或修改 Excel。

## 4. 阶段二：编码与部署

只有用户确认对比表并明确下达“开始工作”后，才能继续：

1. 重新读取最终 Excel，确认没有未讨论的新变化。
2. 更新 `ModConfiguration.cs` 的卡牌 ID、默认开关和设置哈希输入。
3. 更新 `SettingsPanel.cs` 的分类、卡牌名称、调整内容、升级前后效果。
4. 在 `CardPatches.cs` 中按最小范围添加或修改 Harmony 补丁。
5. 若新增自定义 Power 或图标，更新资源、项目文件和部署脚本。
6. 更新 `CustomCardBalance.json` 的版本、描述和卡牌数量。
7. 补充或更新回归检查脚本。
8. 运行固定验证命令和 Release 构建。
9. 确认游戏未运行。
10. 备份旧部署并同步到 Steam `mods` 目录和 D 盘源码镜像。
11. 核对 DLL 与关键文件哈希。

## 5. 固定验证

每次修改至少执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\verify-hotkey-entry.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-settings-layout.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-custom-card-balance-expansion.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-custom-card-balance-rename-and-lazy-ui.ps1
powershell -ExecutionPolicy Bypass -File .\tests\verify-startup-patching.ps1
dotnet build .\CustomCardBalance.csproj -c Release --no-restore
```

Release 构建应为 `0` 警告、`0` 错误。

## 6. 当前 Ver0.106 基线

当前已实现 26 张卡牌：

- 铁甲战士：被遗忘的仪式、怨恨
- 静默猎手：杂技、触不可及、预判、速行者、幽魂形态
- 故障机器人：电流相生、热修复、碎片整理、冷却剂、偏差认知、冰雹风暴、彩虹
- 储君：辉光、星位序列、虚空形态、封印王座
- 亡灵契约师：女妖之嚎、挽歌、降灵、预借时间、摧残、违逆
- 其他：生产制造、未掘宝石

当前已移除：主宰、跃跃欲试、谋杀。

## 7. 联机要求

本 Mod 支持联机，但要求：

- 所有联机玩家都安装 `Custom Card Balance`。
- 所有联机玩家的 26 张卡牌开关设置完全一致。
- 设置哈希参与 gameplay-relevant mod 列表，设置不同会阻止联机兼容。

新增卡牌时必须将其 ID 纳入 `CardIds.All`，否则联机设置哈希不会覆盖该卡牌。

## 8. 部署注意

部署目录：

`<STS2_GAME_DIR>\mods\CustomCardBalance`

部署内容至少包含：

- `CustomCardBalance.dll`
- `CustomCardBalance.json`
- `assets\*.png`

同时同步源码到：

`<SOURCE_MIRROR_DIR>`

本轮 2.3.0 完成后，在用户确认前不得上传 GitHub。
