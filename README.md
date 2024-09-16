# NEUQ 龙芯班管理解决方案

本解决方案包含一切必要代码用于管理 Github Classroom 及 Github Organization。

本解决方案包含以下项目：

## [LoonsonNeuq.Common]

包含一切必要的**通用**工具类，如 GitHub API，Json 模型类，Auth 等。

## [LoonsonNeuq.ListFormatter]

对 `Loongson-Neuq/index` 仓库中的学生列表进行格式化，确保格式正确且 GitHub Id 有效。

##  [LoonsonNeuq.ListFormatter.Tests]

`LoonsonNeuq.ListFormatter` 项目实现的单元测试项目。

## [LoonsonNeuq.AssignmentSubmit]

作业项目中 CI 使用的提交器，包含一个通用自定义评分器以及作业提交器。详细请查看任意作业仓库中 `.assignment/` 下的 `README.md`。

## [LoongsonNeuq.Classroom]

利用 `Loongson-Neuq/index` 仓库中的数据构建 Classroom，接收 `LoonsonNeuq.AssignmentSubmit` 提交的作业，进行认证检测，保存，整合以及 markdown 生成。