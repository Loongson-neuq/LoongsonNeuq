# NEUQ 龙芯班管理解决方案

本解决方案包含一切必要代码用于管理 Github Classroom 及 Github Organization。

本解决方案包含以下项目：

## [LoongsonNeuq.Common]

包含一切必要的**通用**工具类，如 GitHub API，Json 模型类，Auth 等。

## [LoongsonNeuq.ListFormatter]

对 `Loongson-Neuq/index` 仓库中的学生列表进行格式化，确保格式正确且 GitHub Id 有效。

## [LoongsonNeuq.ListFormatter.Tests]

`LoongsonNeuq.ListFormatter` 项目实现的单元测试项目。

## [LoongsonNeuq.AssignmentSubmit]

作业项目中 CI 使用的提交器，包含一个通用自定义评分器以及作业提交器。详细请查看任意作业仓库中 `.assignment/` 下的 `README.md`。

你可以查看这个模板：[AssignmentTemplate](https://github.com/Loongson-neuq/AssignmentTemplate)。同时，下文中有对模板配置文件的介绍。

## [LoongsonNeuq.Classroom]

利用 `Loongson-Neuq/index` 仓库中的数据构建 Classroom，接收 `LoongsonNeuq.AssignmentSubmit` 提交的作业，进行认证检测，保存，整合以及 markdown 生成。

# 作业提交

当作业仓库的远程仓库收到 commit 时，会触发 CI 流程，CI 流程利用 `LoongsonNeuq.AssignmentSubmit` 运行评分器（可选，根据配置需要），然后整合提交结果。

提交结果是一个 json，包含以下字段：

```json
{
  // 作业提交者的 GitHub Id
  "student": "GitHub Id",
  // 归一化作业 Id，根据 作业类型-作业编号 生成，详见 AssignmentConfig.cs
  "assignment_id": "作业 Id",
  // 作业提交时间，UTC Unix 时间戳
  "timestamp": 1234567890,
  // 作业提交的仓库，完整形式 `GitHub Id/作业仓库名`
  "assignment_repo": "作业仓库地址",
  // 作业提交的 commit sha
  "repo_sha": "提交的 commit sha",
  // 用于储存额外信息的分支，可选
  "info_branch": null,
  // 用于储存额外信息的 commit sha，可选
  "info_sha": null,
  "log_artifact_url": "提交的 log 文件地址",
  "score": [
    {
      "title": "测试点标题",
      "score": 100,
    }
  ]
}
```

该 json 被生成后会被上传到 Artifacts。然后向 `LoongsonNeuq.Classroom` 发送一个请求触发 CI 流程。触发流程时，传递该 json 的 Artifacts 地址，以及仓库地址（`<GitHub-Id>/作业仓库`）。

其中，所有的 Artifacts 都不会完整传递，而是省略了 `https://github.com/user/repo/` 部分，在 `LoongsonNeuq.Classroom` 中会根据提交的仓库地址自动补全。这是为了在一定程度上防止伪造提交。当 `LoongsonNeuq.Classroom` 发现提交的仓库地址与提交的 Artifacts 地址不匹配时，会认为这是一次伪造提交，拒绝处理。

由于 `LoongsonNeuq.AssignmentSubmit` 除了等待 `LoongsonNeuq.Classroom` CI 以外无法确认作业是否会被各种原因拒绝，所以即使 `LoongsonNeuq.AssignmentSubmit` CI 通过，也不代表作业会被接受。当然，为了方便，我们会允许 `LoongsonNeuq.AssignmentSubmit` manual dispatch CI，以便失败时重新提交。

## LoongsonNeuq.Classroom 工作流

`LoongsonNeuq.Classroom` 会在收到提交请求后，根据提交的仓库地址，检查是否在 `Loongson-Neuq/index` 仓库中，如果不在，会拒绝处理。接下来，拉取储存提交信息的 Artifacts 以及作业仓库 `.assignment` 中的配置文件。并对相关结果进行匹配检查，如果不匹配，会拒绝处理。

检查 repo 与提交者是否匹配，如果不匹配，拒绝处理。

检查作业仓库中 `.assignment` 和 `.github` 文件夹是否被修改，如果被修改，拒绝处理，因为修改这些文件夹可以实现伪造提交。

检查完成后，会正式开始处理作业提交。

首先，会在新的文件夹里建立 git 仓库然后 checkout 到**另一分支**，设置远程源为本仓库，并拉取。该分支会按照树形结果将所有提交的结果持久化储存起来。接下来，会按照树形结构将本次提交的结果也一同持久化储存起来。最后，会将提交的结果整合到树形结构中，然后提交到远程源。

该树形结构按照以下结构存储：

```
- root
    - 作业类型（OS 或 CPU）/
        - 作业编号/
            - 学生 GitHub Id/
                - 提交次数序号/
                    - submit.json
                    - log.txt（可能不存在）
```

其中 `submit.json` 大致与提交结果相同，但是会包含额外的信息，如提交次数序号。不包含 `log-artifact-url` 字段，但会包含一个字段 `has_log_url` 指示是否有 log 文件。

仓库根目录会包含一个 `index.json` 文件，用于记录所有的作业信息，包括作业类型，作业编号，在 GitHub Classroom 中的 Id。结构如下

```json
{
  "assignments": [
    {
      "type": "作业类型",
      "assignment_id": "作业 Id",
      "classroom_id": "GitHub Classroom 中的作业 Id"
    }
  ]
}
```

每个作业文件夹下也会包含一个 `index.json` 文件，格式如下：

```json
{
  "assignment_id": "作业 Id",
  "classroom_id": "GitHub Classroom 中的作业 Id",
  "accepted": [
    {
      // 注：由 GitHub Classroom API 获取的该字段就是一个向量，一般只有一个值，在处理时通常也只取第一个
      "student": ["GitHub Id"],
      "repo": "<user>/<repo>",
      "commit_count": 0,
      // 指示最新一次提交的文件夹（上面的序号），如果没提交过则为 null
      "latest": null
    }
  ]
}
```

提交到远程源后，可以设置其他步骤触发其他 CI 或是服务器以便利用这些信息进行进一步处理，例如生成 markdown 文件。

例如，根据 `LoongsonNeuq/index` 中的学生列表以及以上信息，可以构建一个表格，表示每为学生的作业提交情况，例如：

| GitHub Id | 作业 1 | 作业 2 | 作业 3 |
| --------- | ----- | ----- | ------ |
| Cai1Hsu | 已提交 | 已领取 | 未领取 |

# 仓库中作业信息对应问题及解决方案

当新创建一个作业后，虽然 `LoongsonNeuq.Classroom` 可以通过 Classroom API 获取到所有作业的信息，但是无法与已有的信息对应起来，例如（作业Id）。这主要是因为我们通过作业仓库中的 `.assignment` 文件夹中的配置文件来获取作业信息。因此我们需要获取到作业仓库中的 `.assignment` 文件夹中的配置文件，然后将其与 Classroom API 获取到的信息对应起来。

我们使用这个 API 来解决这个问题：`https://api.github.com/assignments/<CLASSROOM_ASSIGNMENT_ID>`

样例返回：

```json
{
  "id": 123456,
  "public_repo": true,
  "title": "OS 测试作业",
  "type": "individual",
  "invite_link": "https://classroom.github.com/a/123456",
  "invitations_enabled": true,
  "slug": "os",
  "students_are_repo_admins": false,
  "feedback_pull_requests_enabled": false,
  "max_teams": null,
  "max_members": null,
  "editor": null,
  "accepted": 2,
  "submissions": 0,
  "passing": 0,
  "language": null,
  "deadline": null,
  "classroom": {
    "id": 123456,
    "name": "2024 NEUQ 龙芯班",
    "archived": false,
    "url": "https://classroom.github.com/classrooms/123456"
  },
  "starter_code_repository": {
    "id": 123456,
    "name": "2024-neuq-os-AssignmentTemplate",
    "full_name": "Loongson-neuq/2024-neuq-os-AssignmentTemplate",
    "html_url": "https://github.com/Loongson-neuq/2024-neuq-os-AssignmentTemplate",
    "node_id": "123456",
    "private": false,
    "default_branch": "master"
  }
}
```

利用`starter_code_repository`中的信息，我们可以获取到本次作业的模板仓库，通过模板仓库，我们就能将作业仓库中的 `.assignment` 文件夹中的配置文件与 Classroom API 获取到的信息对应起来。

这会在每一次运行 `LoongsonNeuq.Classroom` 时行时自动进行，以构建正确的作业提交树形结构。

# GitHub API

GitHub API 已经全部迁移至由源生成器构建的 SDK，调用风格完全遵循 RESTful 风格。

你可以查看 [Migrate octokit.NET to the new generated SDK](https://github.com/Loongson-neuq/LoongsonNeuq/pull/2) 中的信息来了解更多。

由于 SDK 与 GitHub OpenAPI 完全对应，仅有大小写不同，因此你只需要查看[官方文档](https://docs.github.com/en/rest)即可。

使用时，通过 Ioc 容器解析 `GitHubClient` 对象，通过该对象调用 API。Ioc 容器具有自动装配功能，通常情况下无需手动解析，默认情况下使用构造函数注入依赖。

注意，在调用 `GetAsync`，`PostAsync` 等谓词性 方法前，请求都不会被发送，只是根据参数构建请求对象。调用方法时，请求会被发送，并返回结果对象。

## 自动评分

`LoongsonNeuq.Autograder` 项目是一个自动评分器，可以根据配置文件自动评分。配置文件位于每一份作业的 `.assignment/config.json`。

配置文件大致如下：

```json
{
  "auto_grade": {
    "enable": false,
    "upload_output": true,
    "steps": [
      {
        "title": "Step 1",
        "timeout": 60,
        "command": "make test",
        "score": 100,
      }
    ]
  }
}
```

架构如下：

- `enable`: bool 是否启用自动评分
- `upload_output`: bool 是否上传评分结果
- `steps`: 评分步骤
  - `timeout`: double 评分步骤超时时间, 单位秒
  - `command`: string 评分步骤命令
  - `score`: int 该测试点的分数，通过为满分，不通过则为 0

steps 是一个向量，因此你可以设置多个评分步骤。评分时，步骤会依次运行，如果某一步骤超时或是返回非 0 值，会停止评分，并标记该测试点为 0 分。如果需要对测试进行输入输出检查，你需要先编写脚本，然后在 `command` 中调用。

`command` 会被储存在一个临时的 .sh 文件中，然后调用 shell 执行。通常情况下，你可以假设 `cwd` 为仓库根目录。如果是本地调试，则为 `Autograder` 自身的 `cwd`。

## 作业配置

作业配置文件位于每一份作业的 `.assignment/config.json`。

以下是一个样例配置文件：

```json
{
  "name": "第一次作业",
  "description": "第一次作业的描述，详细查看仓库根目录的 README.md",
  "type": "OS",
  "status": "open",
  "id": "week-1",
  "version": 1,
  "auto_grade": {
    "enable": false,
    "upload_output": true,
    "steps": [
      {
        "title": "Step 1",
        "timeout": 60,
        "command": "make test",
        "score": 100,
      }
    ]
  }
}
```

作业配置文件，用于标识作业的元信息。**每次布置新作业时需要更新该文件。**
请注意格式的正确性，因此请仔细阅读以下说明。
可以使用 `check_config.py` 脚本进行格式检查。

该文件包含以下键：

### `name`
- 取值范围：`字符串`

作业名称，标识作业的名字，仅用于本地

### `description`
- 取值范围：`字符串`

作业描述，应该没什么用，仅用于本地

### `type`
- 取值范围：`字符串`

作业类型，标识该作业属于哪个方向。可选值: "OS", "CPU"

学生只需要选择对应的方向提交作业即可。

用于服务器端的作业分类。

### `status`
- 取值范围：`字符串`

标识作业状态，可选值: "open", "closed"

### `id`
- 取值范围：`字符串`

标识作业的唯一ID，与`type`字段一同用于用于服务器端的作业分类。

**请不要包含空格或非ASCII字符**

### `version`
- 取值范围：`整形数字`

作业版本，用于标识作业的版本号。通常为 1 即可。用于服务端和本地的作业版本控制。有版本更新时可以递增。提交时如果本地版本低于服务器版本会提示更新。同时作业会被远程拒绝。

### `config_version`

配置文件架构版本，为了可能存在的向后兼容性，配置文件架构版本号。目前为 1。后端在读取配置文件时可能会根据版本号进行不同的处理。

### 自动评分

其他字段用于自动评分，详见上文。这里不再赘述。

#### `upload_output`
- 取值范围：`bool`

标识是否上传评分结果。如果为 `true`，则会将评分结果上传到 Github Actions 的 Artifact 中。
