# SDK 术语表

[English](../en/glossary.md)

## SDK

软件开发工具包（Software Development Kit）。本仓库提供编写 Quantum 插件所需的类型和工具函数。

## ABI

应用二进制接口（Application Binary Interface）。编译后的 .NET 插件与宿主共享的程序集和类型标识。

## RPC

远程过程调用（Remote Procedure Call）。Quantum 按名称路由调用，并在插件运行时之间序列化请求和响应。

## DI

依赖注入（Dependency Injection）。.NET 插件从自己的运行时容器获取服务。

## iframe

浏览器内嵌框架。Quantum 在沙箱框架中运行 Web 插件，通过消息提供宿主服务。

## SemVer

语义化版本（Semantic Versioning）。两套插件 SDK 使用的版本组成与比较规则。

## Host

宿主。指加载插件并实现 SDK 所声明服务的 Quantum Desktop。
