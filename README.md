# Amis Admin .NET Core

这是一个基于 ASP.NET Core 的最小可运行示例，用 C# 在后端生成 [amis](https://aisuda.bce.baidu.com/amis/zh-CN/index) JSON schema，
实现类似 `amisadmin/fastapi-amis-admin` 的 admin 集成思路：

- 不改官方 amis JavaScript UI 组件库；
- 后端提供 admin 页面 schema；
- 后端提供基础 CRUD API；
- 前端宿主页只负责加载官方 amis SDK 并渲染后端返回的 schema。

## 运行

```bash
dotnet run --project /home/runner/work/amis-admin-dotnet/amis-admin-dotnet/src/AmisAdminDotNet/AmisAdminDotNet.csproj
```

启动后访问：

- `http://localhost:5015/admin`

## 当前实现范围

当前仓库原本几乎为空，因此本次提交先落一个最小迁移骨架：

- 一个 ASP.NET Core Web 项目；
- 一个后端生成 amis schema 的 admin 页面；
- 一个内存版用户管理 CRUD 示例；
- 一组聚焦于 schema 生成和 CRUD 行为的 xUnit 测试。
